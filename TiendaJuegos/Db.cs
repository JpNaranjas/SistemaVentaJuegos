using System.Configuration;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace TiendaJuegos;

/// <summary>Datos de la sesión actual. Perfil cambia según el botón de login usado.</summary>
public static class Sesion
{
    public static string Usuario = "";
    public static string Password = "";
    public static string Perfil = "";          // "Administrador" | "Operador"
    public static bool EsAdmin => Perfil == "Administrador";
}

/// <summary>Capa de acceso a datos (DAO). Todo el SQL es manual y parametrizado.</summary>
public static class Db
{
    public static event Action<string> Log;
    static void Escribir(string s) => Log?.Invoke(s);

    // ---------- Conexión ----------
    static string Cadena(string usuario, string pass)
    {
        string baseCs = ConfigurationManager.ConnectionStrings["TiendaJuegos"].ConnectionString;
        var b = new MySqlConnectionStringBuilder(baseCs) { UserID = usuario, Password = pass };
        return b.ConnectionString;
    }

    public static MySqlConnection Abrir(string usuario, string pass)
    {
        var c = new MySqlConnection(Cadena(usuario, pass));
        try
        {
            c.Open();
            using var cmd = new MySqlCommand("SET ROLE ALL", c);   // activa los roles asignados
            cmd.ExecuteNonQuery();
        }
        catch { c.Dispose(); throw; }
        return c;
    }

    public static MySqlConnection Abrir() => Abrir(Sesion.Usuario, Sesion.Password);

    /// <summary>Roles activos de la sesión (después de SET ROLE ALL).</summary>
    public static string RolesActivos(MySqlConnection c)
    {
        using var cmd = new MySqlCommand("SELECT CURRENT_ROLE()", c);
        return Convert.ToString(cmd.ExecuteScalar()) ?? "";
    }

    public static bool TieneRol(MySqlConnection c, string rol) =>
        RolesActivos(c).Contains(rol, StringComparison.OrdinalIgnoreCase);

    // ---------- Utilidades ----------
    public static MySqlParameter P(string nombre, object valor) =>
        new MySqlParameter { ParameterName = nombre, Value = valor ?? DBNull.Value };

    public static MySqlParameter Salida(string nombre, int tam) =>
        new MySqlParameter { ParameterName = nombre, MySqlDbType = MySqlDbType.VarChar, Size = tam, Direction = ParameterDirection.Output };

    static string Describir(string sql, IEnumerable<MySqlParameter> ps)
    {
        var sb = new StringBuilder(sql.Replace("\r", " ").Replace("\n", " "));
        var lista = ps.Where(p => p.Direction != ParameterDirection.Output)
                      .Select(p => p.ParameterName + "=" + (p.Value is DBNull ? "NULL" : p.Value)).ToList();
        if (lista.Count > 0) sb.Append("   -- [" + string.Join(", ", lista) + "]");
        return sb.ToString();
    }

    // ---------- Consultas / DML ----------
    public static DataTable Consulta(string sql, params MySqlParameter[] ps)
    {
        using var c = Abrir();
        using var cmd = new MySqlCommand(sql, c);
        cmd.Parameters.AddRange(ps);
        Escribir("SELECT> " + Describir(sql, ps));
        using var rd = cmd.ExecuteReader();
        var dt = new DataTable();
        dt.Load(rd);
        return dt;
    }

    public static int Ejecutar(string sql, params MySqlParameter[] ps)
    {
        using var c = Abrir();
        using var cmd = new MySqlCommand(sql, c);
        cmd.Parameters.AddRange(ps);
        Escribir("DML> " + Describir(sql, ps));
        return cmd.ExecuteNonQuery();
    }

    public static object Escalar(string sql, params MySqlParameter[] ps)
    {
        using var c = Abrir();
        using var cmd = new MySqlCommand(sql, c);
        cmd.Parameters.AddRange(ps);
        Escribir("SCALAR> " + Describir(sql, ps));
        return cmd.ExecuteScalar();
    }

    // ---------- Stored procedures ----------
    /// <summary>Invoca un SP con CommandType.StoredProcedure. Devuelve el parámetro OUT (si existe).</summary>
    public static string Procedimiento(string nombre, params MySqlParameter[] ps)
    {
        using var c = Abrir();
        using var cmd = new MySqlCommand(nombre, c) { CommandType = CommandType.StoredProcedure };
        foreach (var p in ps)
            if (!p.ParameterName.StartsWith("@")) p.ParameterName = "@" + p.ParameterName;
        cmd.Parameters.AddRange(ps);
        Escribir("CALL> " + Describir(nombre, ps));
        cmd.ExecuteNonQuery();
        foreach (MySqlParameter p in cmd.Parameters)
            if (p.Direction == ParameterDirection.Output) return Convert.ToString(p.Value) ?? "";
        return "";
    }

    // ---------- Transacción desde C# (2 tablas: juegos + pagos) ----------
    static int ExecTx(MySqlConnection c, MySqlTransaction tx, string sql, params MySqlParameter[] ps)
    {
        using var cmd = new MySqlCommand(sql, c, tx);
        cmd.Parameters.AddRange(ps);
        Escribir("TX> " + Describir(sql, ps));
        return cmd.ExecuteNonQuery();
    }

    public static string VentaConTransaccion(int cliente, int producto, int cantidad, string metodo, bool forzarError)
    {
        using var c = Abrir();
        using var tx = c.BeginTransaction();
        Escribir("TX> START TRANSACTION");
        try
        {
            decimal precio; int stock;
            using (var cmd = new MySqlCommand(
                "SELECT precio_unitario, stock FROM juegos WHERE producto_id = @p FOR UPDATE", c, tx))
            {
                cmd.Parameters.Add(P("@p", producto));
                Escribir("TX> SELECT precio_unitario, stock FROM juegos WHERE producto_id=@p FOR UPDATE");
                using var rd = cmd.ExecuteReader();
                if (!rd.Read()) throw new InvalidOperationException("El juego no existe.");
                precio = rd.GetDecimal(0);
                stock = rd.GetInt32(1);
            }
            if (stock < cantidad) throw new InvalidOperationException("Stock insuficiente.");

            ExecTx(c, tx, "UPDATE juegos SET stock = stock - @c WHERE producto_id = @p",
                   P("@c", cantidad), P("@p", producto));

            if (forzarError) throw new InvalidOperationException("Error simulado después de descontar el stock.");

            ExecTx(c, tx,
                "INSERT INTO pagos (cliente_id, producto_id, cantidad, total, metodo_pago, estado) VALUES (@cli,@p,@c,@t,@m,'Pagado')",
                P("@cli", cliente), P("@p", producto), P("@c", cantidad), P("@t", precio * cantidad), P("@m", metodo));

            tx.Commit();
            Escribir("TX> COMMIT");
            return "COMMIT: venta registrada. Total $" + (precio * cantidad);
        }
        catch
        {
            tx.Rollback();
            Escribir("TX> ROLLBACK");
            throw;
        }
    }

    // ---------- Mensajes comprensibles ----------
    public static string Mensaje(Exception ex)
    {
        if (ex is MySqlException m)
        {
            switch (m.Number)
            {
                case 1045: return "Usuario o contraseña incorrectos.";
                case 1042:
                case 2002: return "No se pudo conectar al servidor MySQL. Verifica que el servicio esté activo.";
                case 1044:
                case 1142:
                case 1143:
                case 1227:
                case 1370: return "PERMISO DENEGADO: tu perfil (" + Sesion.Perfil + ") no tiene autorización para realizar esta operación.";
                case 1451: return "No se puede eliminar/modificar: el registro tiene datos relacionados (violación de llave foránea).";
                case 1452: return "El cliente o juego indicado no existe (violación de llave foránea).";
                case 1062: return "Ya existe un registro con ese valor único (nombre o correo duplicado).";
                case 3819: return "El valor no cumple una restricción CHECK (precio > 0, stock >= 0, cantidad > 0...).";
                case 1644: return m.Message;   // SIGNAL del trigger de regla de negocio
                default: return "Error de base de datos (" + m.Number + "): " + m.Message;
            }
        }
        return ex.Message;
    }
}
