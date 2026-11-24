using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace TiendaJuegos;

/// <summary>Menú principal: las pestañas cambian según el perfil iniciado.</summary>
public class Form2 : Form
{
    TextBox txtCuenta, txtLog;
    TabControl tabs;
    readonly Dictionary<TabPage, Action> refrescar = new Dictionary<TabPage, Action>();

    public Form2()
    {
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 11F);
        Text = "Aura Gaming - " + Sesion.Usuario + " (" + Sesion.Perfil + ")";
        Width = 1400; Height = 900;
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;

        // ---------- Barra superior ----------
        var morado = Color.FromArgb(74, 20, 140);
        var top = new Panel { Dock = DockStyle.Top, Height = 128, BackColor = morado };

        var lblNombre = new Label
        {
            Text = "AURA GAMING", Font = new Font("Segoe UI Black", 30F, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 214, 0), AutoSize = true, Left = 20, Top = 14
        };

        var lblBienvenida = new Label
        {
            Text = "Bienvenido, " + Sesion.Usuario, ForeColor = Color.White, AutoSize = true,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold), Margin = new Padding(3, 0, 3, 6)
        };
        var lblCuenta = new Label { Text = "Sesión iniciada con:", ForeColor = Color.White, AutoSize = true, Margin = new Padding(3, 8, 3, 0) };
        txtCuenta = new TextBox { Width = 440, ReadOnly = true, Margin = new Padding(3, 5, 10, 0) };
        txtCuenta.Text = Sesion.Perfil + "  |  usuario BD: " + Sesion.Usuario;   // depende del botón usado en Form1
        var btnSalir = new Button { Text = "Cerrar sesión", Width = 150, Height = 34, ForeColor = Color.Black, BackColor = Color.White, Margin = new Padding(3, 3, 3, 0) };
        btnSalir.Click += (s, e) => Close();

        var fila = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Margin = new Padding(0) };
        fila.Controls.AddRange(new Control[] { lblCuenta, txtCuenta, btnSalir });
        var der = new FlowLayoutPanel
        {
            Dock = DockStyle.Right, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 10, 20, 0)
        };
        der.Controls.Add(lblBienvenida);
        der.Controls.Add(fila);
        top.Controls.Add(der);
        top.Controls.Add(lblNombre);

        txtLog = new TextBox { Dock = DockStyle.Bottom, Height = 130, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 10F) };

        tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12F, FontStyle.Bold), SizeMode = TabSizeMode.Fixed };
        Load += (s, e) => tabs.ItemSize = new Size(LogicalToDeviceUnits(230), LogicalToDeviceUnits(46));

        Controls.Add(tabs);
        Controls.Add(txtLog);
        Controls.Add(top);

        Db.Log += AppendLog;
        FormClosed += (s, e) => Db.Log -= AppendLog;
        AppendLog("Sesión iniciada: " + Sesion.Perfil + " (" + Sesion.Usuario + "). Aquí se muestran las operaciones SQL ejecutadas.");

        // ---- Opciones según el perfil (lo que su ROL permite en el script SQL) ----
        if (Sesion.EsAdmin)
        {
            Agregar(TabJuegos(true));
            Agregar(TabClientes(true));
            Agregar(TabReservas(true));
            Agregar(TabVentas(true));
            Agregar(TabStock());
            Agregar(TabReportes());
        }
        else
        {
            Agregar(TabJuegos(false));
            Agregar(TabClientes(false));
            Agregar(TabReservas(false));
            Agregar(TabVentas(false));
            Agregar(TabPermisos());
        }

        tabs.SelectedIndexChanged += (s, e) =>
        {
            if (tabs.SelectedTab != null && refrescar.TryGetValue(tabs.SelectedTab, out var a)) Run(a);
        };
    }

    void Agregar(TabPage tp)
    {
        // Las pestañas heredan la fuente grande/negrita del TabControl; se restablece para su contenido
        tp.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
        tabs.TabPages.Add(tp);
    }

    // ===================== Helpers de UI =====================
    void AppendLog(string s) => txtLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + s + Environment.NewLine);

    void Run(Action a)
    {
        try { a(); }
        catch (Exception ex)
        {
            AppendLog("ERROR> " + ex.Message);
            MessageBox.Show(Db.Mensaje(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    static void Aviso(string m) => MessageBox.Show(m, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    static void Info(string m) => MessageBox.Show(m, "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);

    static FlowLayoutPanel Flow() => new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(5) };

    static void Sec(FlowLayoutPanel fp, string t)
    {
        var l = new Label { Text = t, AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Margin = new Padding(3, 10, 3, 4) };
        fp.Controls.Add(l);
        fp.SetFlowBreak(l, true);
    }

    static Panel F(string etiqueta, Control c, int w)
    {
        w = (int)(w * 1.15);
        var p = new Panel { Width = w + 12, Height = 66 };
        p.Controls.Add(new Label { Text = etiqueta, AutoSize = true, Left = 2, Top = 2 });
        c.Left = 2; c.Top = 28; c.Width = w;
        p.Controls.Add(c);
        return p;
    }

    static Panel BP(string texto, EventHandler h, int w = 130)
    {
        w = (int)(w * 1.15);
        var p = new Panel { Width = w + 12, Height = 66 };
        var b = new Button { Text = texto, Width = w, Height = 34, Left = 2, Top = 24 };
        b.Click += h;
        p.Controls.Add(b);
        return p;
    }

    static DataGridView Grid() => new DataGridView
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells, BackgroundColor = SystemColors.Window,
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize, AllowUserToResizeRows = false
    };

    static NumericUpDown Num(decimal max, int dec = 0) => new NumericUpDown { Maximum = max, DecimalPlaces = dec };

    static ComboBox Cmb(params string[] items)
    {
        var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        c.Items.AddRange(items);
        if (items.Length > 0) c.SelectedIndex = 0;
        return c;
    }

    static void CargarCombo(ComboBox c, string sql)
    {
        var dt = Db.Consulta(sql);
        c.DisplayMember = "txt";
        c.ValueMember = "id";
        c.DataSource = dt;
    }

    const string SqlJuegosCombo = "SELECT producto_id AS id, CONCAT(nombre,' - $',precio_unitario,' (stock ',stock,')') AS txt FROM juegos ORDER BY nombre";
    const string SqlClientesCombo = "SELECT cliente_id AS id, nombre AS txt FROM clientes ORDER BY nombre";

    static string S(DataGridViewRow r, string col) => Convert.ToString(r.Cells[col].Value) ?? "";

    // ===================== JUEGOS =====================
    DataGridView gJ;
    TextBox fJNom, fJMarca, jNombre, jMarca, jConsola;
    ComboBox fJCat, fJPlat, fJOrden, jCat;
    NumericUpDown fJPmin, fJPmax, jPrecio, jStock;
    CheckBox fJStock;
    DateTimePicker jFecha;
    int jId = 0;

    TabPage TabJuegos(bool edit)
    {
        var tp = new TabPage(edit ? "Juegos (CRUD)" : "Catálogo de juegos");
        gJ = Grid();
        tp.Controls.Add(gJ);
        var fp = Flow();

        Sec(fp, "Filtros: nombre/marca (LIKE), precio (BETWEEN), plataforma (IN), categoría");
        fJNom = new TextBox(); fJMarca = new TextBox();
        fJCat = Cmb("Todas", "Videojuego", "Juego de mesa", "Otros");
        fJPmin = Num(100000, 2); fJPmax = Num(100000, 2); fJPmax.Value = 100000;
        fJPlat = Cmb("Todas", "PlayStation", "Nintendo", "Xbox");
        fJOrden = Cmb("Nombre A-Z", "Precio: menor a mayor", "Precio: mayor a menor", "Stock: mayor a menor");
        fJStock = new CheckBox { Text = "Solo con stock", AutoSize = true };
        fp.Controls.AddRange(new Control[]
        {
            F("Nombre contiene", fJNom, 150), F("Marca contiene", fJMarca, 120), F("Categoría", fJCat, 120),
            F("Precio mín.", fJPmin, 90), F("Precio máx.", fJPmax, 90), F("Plataforma", fJPlat, 110),
            F("Ordenar por", fJOrden, 160), F("", fJStock, 110),
            BP("Buscar", (s, e) => Run(BuscarJuegos), 90),
            BP("Quitar filtros", (s, e) => Run(() =>
            {
                fJNom.Clear(); fJMarca.Clear(); fJCat.SelectedIndex = 0; fJPmin.Value = 0; fJPmax.Value = 100000;
                fJPlat.SelectedIndex = 0; fJOrden.SelectedIndex = 0; fJStock.Checked = false; BuscarJuegos();
            }), 110)
        });

        if (edit)
        {
            Sec(fp, "Datos del juego (selecciona una fila para modificar o eliminar)");
            jNombre = new TextBox(); jMarca = new TextBox(); jConsola = new TextBox();
            jCat = Cmb("Videojuego", "Juego de mesa", "Otros");
            jPrecio = Num(100000, 2); jStock = Num(100000);
            jFecha = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
            fp.Controls.AddRange(new Control[]
            {
                F("Nombre", jNombre, 200), F("Categoría", jCat, 120), F("Marca", jMarca, 120), F("Consola (opcional)", jConsola, 130),
                F("Precio", jPrecio, 90), F("Stock", jStock, 70), F("Fecha de salida", jFecha, 130),
                BP("Agregar", (s, e) => Run(() => GuardarJuego(true)), 90),
                BP("Modificar", (s, e) => Run(() => GuardarJuego(false)), 90),
                BP("Eliminar", (s, e) => Run(EliminarJuego), 90),
                BP("Limpiar", (s, e) => LimpiarJuego(), 80)
            });
            gJ.SelectionChanged += (s, e) => SelJuego();
        }

        tp.Controls.Add(fp);
        refrescar[tp] = BuscarJuegos;
        Run(BuscarJuegos);
        return tp;
    }

    void BuscarJuegos()
    {
        var sql = new StringBuilder("SELECT producto_id AS ID, nombre, categoria, marca, consola, precio_unitario AS precio, stock, fecha_salida FROM juegos WHERE 1=1");
        var ps = new List<MySql.Data.MySqlClient.MySqlParameter>();
        if (fJNom.Text.Trim() != "") { sql.Append(" AND nombre LIKE @n"); ps.Add(Db.P("@n", "%" + fJNom.Text.Trim() + "%")); }
        if (fJMarca.Text.Trim() != "") { sql.Append(" AND marca LIKE @m"); ps.Add(Db.P("@m", "%" + fJMarca.Text.Trim() + "%")); }
        if (fJCat.SelectedIndex > 0) { sql.Append(" AND categoria = @cat"); ps.Add(Db.P("@cat", fJCat.Text)); }
        sql.Append(" AND precio_unitario BETWEEN @pmin AND @pmax");
        ps.Add(Db.P("@pmin", fJPmin.Value)); ps.Add(Db.P("@pmax", fJPmax.Value));
        if (fJPlat.SelectedIndex > 0)
        {
            string[] consolas = fJPlat.Text switch
            {
                "PlayStation" => new[] { "PlayStation 4", "PlayStation 5" },
                "Nintendo" => new[] { "Nintendo Switch", "Nintendo Switch 2" },
                _ => new[] { "Xbox Series X", "Xbox One" }
            };
            var nombres = new List<string>();
            for (int i = 0; i < consolas.Length; i++) { nombres.Add("@c" + i); ps.Add(Db.P("@c" + i, consolas[i])); }
            sql.Append(" AND consola IN (" + string.Join(",", nombres) + ")");
        }
        if (fJStock.Checked) sql.Append(" AND stock > 0");
        sql.Append(fJOrden.SelectedIndex switch
        {
            1 => " ORDER BY precio_unitario ASC",
            2 => " ORDER BY precio_unitario DESC",
            3 => " ORDER BY stock DESC",
            _ => " ORDER BY nombre ASC"
        });
        gJ.DataSource = Db.Consulta(sql.ToString(), ps.ToArray());
    }

    void SelJuego()
    {
        var r = gJ.CurrentRow;
        if (r == null || jNombre == null) return;
        jId = Convert.ToInt32(r.Cells["ID"].Value);
        jNombre.Text = S(r, "nombre"); jMarca.Text = S(r, "marca"); jConsola.Text = S(r, "consola");
        jCat.SelectedItem = S(r, "categoria");
        jPrecio.Value = Convert.ToDecimal(r.Cells["precio"].Value);
        jStock.Value = Convert.ToDecimal(r.Cells["stock"].Value);
        if (r.Cells["fecha_salida"].Value is DBNull) jFecha.Checked = false;
        else { jFecha.Checked = true; jFecha.Value = Convert.ToDateTime(r.Cells["fecha_salida"].Value); }
    }

    void LimpiarJuego()
    {
        jId = 0; jNombre.Clear(); jMarca.Clear(); jConsola.Clear(); jCat.SelectedIndex = 0;
        jPrecio.Value = 0; jStock.Value = 0; jFecha.Checked = false;
    }

    void GuardarJuego(bool nuevo)
    {
        if (string.IsNullOrWhiteSpace(jNombre.Text) || string.IsNullOrWhiteSpace(jMarca.Text)) { Aviso("Nombre y marca son obligatorios."); return; }
        if (jPrecio.Value <= 0) { Aviso("El precio debe ser mayor a 0."); return; }
        var consola = string.IsNullOrWhiteSpace(jConsola.Text) ? (object)DBNull.Value : jConsola.Text.Trim();
        var fecha = jFecha.Checked ? (object)jFecha.Value.Date : DBNull.Value;
        if (nuevo)
        {
            Db.Ejecutar("INSERT INTO juegos (nombre, precio_unitario, categoria, marca, consola, stock, fecha_salida) VALUES (@n,@p,@c,@m,@co,@s,@f)",
                Db.P("@n", jNombre.Text.Trim()), Db.P("@p", jPrecio.Value), Db.P("@c", jCat.Text), Db.P("@m", jMarca.Text.Trim()),
                Db.P("@co", consola), Db.P("@s", (int)jStock.Value), Db.P("@f", fecha));
            Info("Juego agregado.");
        }
        else
        {
            if (jId == 0) { Aviso("Selecciona un juego de la tabla."); return; }
            Db.Ejecutar("UPDATE juegos SET nombre=@n, precio_unitario=@p, categoria=@c, marca=@m, consola=@co, stock=@s, fecha_salida=@f WHERE producto_id=@id",
                Db.P("@n", jNombre.Text.Trim()), Db.P("@p", jPrecio.Value), Db.P("@c", jCat.Text), Db.P("@m", jMarca.Text.Trim()),
                Db.P("@co", consola), Db.P("@s", (int)jStock.Value), Db.P("@f", fecha), Db.P("@id", jId));
            Info("Juego modificado.");
        }
        BuscarJuegos();
    }

    void EliminarJuego()
    {
        if (jId == 0) { Aviso("Selecciona un juego de la tabla."); return; }
        if (MessageBox.Show("¿Eliminar el juego seleccionado?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        Db.Ejecutar("DELETE FROM juegos WHERE producto_id=@id", Db.P("@id", jId));
        jId = 0; Info("Juego eliminado.");
        BuscarJuegos();
    }

    // ===================== CLIENTES =====================
    DataGridView gC;
    TextBox fCNom, fCCorreo, cNombre, cCorreo, cTel;
    CheckBox fCRango;
    DateTimePicker fCDesde, fCHasta, cFecha;
    int cId = 0;

    TabPage TabClientes(bool edit)
    {
        var tp = new TabPage(edit ? "Clientes (CRUD)" : "Clientes (consulta)");
        gC = Grid();
        tp.Controls.Add(gC);
        var fp = Flow();

        Sec(fp, "Filtros: nombre/correo (LIKE) y rango de fecha de alta (BETWEEN)");
        fCNom = new TextBox(); fCCorreo = new TextBox();
        fCRango = new CheckBox { Text = "Usar rango de fechas", AutoSize = true };
        fCDesde = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = new DateTime(2026, 1, 1) };
        fCHasta = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
        fp.Controls.AddRange(new Control[]
        {
            F("Nombre contiene", fCNom, 150), F("Correo contiene", fCCorreo, 160), F("", fCRango, 150),
            F("Alta desde", fCDesde, 110), F("Alta hasta", fCHasta, 110),
            BP("Buscar", (s, e) => Run(BuscarClientes), 90),
            BP("Quitar filtros", (s, e) => Run(() => { fCNom.Clear(); fCCorreo.Clear(); fCRango.Checked = false; BuscarClientes(); }), 110)
        });

        if (edit)
        {
            Sec(fp, "Datos del cliente (selecciona una fila para modificar o eliminar)");
            cNombre = new TextBox(); cCorreo = new TextBox(); cTel = new TextBox();
            cFecha = new DateTimePicker { Format = DateTimePickerFormat.Short };
            fp.Controls.AddRange(new Control[]
            {
                F("Nombre", cNombre, 200), F("Correo", cCorreo, 200), F("Teléfono", cTel, 110), F("Fecha de alta", cFecha, 110),
                BP("Agregar", (s, e) => Run(() => GuardarCliente(true)), 90),
                BP("Modificar", (s, e) => Run(() => GuardarCliente(false)), 90),
                BP("Eliminar", (s, e) => Run(EliminarCliente), 90),
                BP("Limpiar", (s, e) => { cId = 0; cNombre.Clear(); cCorreo.Clear(); cTel.Clear(); cFecha.Value = DateTime.Today; }, 80)
            });
            gC.SelectionChanged += (s, e) =>
            {
                var r = gC.CurrentRow; if (r == null) return;
                cId = Convert.ToInt32(r.Cells["ID"].Value);
                cNombre.Text = S(r, "nombre"); cCorreo.Text = S(r, "correo"); cTel.Text = S(r, "telefono");
                cFecha.Value = Convert.ToDateTime(r.Cells["fecha_alta"].Value);
            };
        }

        tp.Controls.Add(fp);
        refrescar[tp] = BuscarClientes;
        Run(BuscarClientes);
        return tp;
    }

    void BuscarClientes()
    {
        var sql = new StringBuilder("SELECT cliente_id AS ID, nombre, correo, telefono, fecha_alta FROM clientes WHERE 1=1");
        var ps = new List<MySql.Data.MySqlClient.MySqlParameter>();
        if (fCNom.Text.Trim() != "") { sql.Append(" AND nombre LIKE @n"); ps.Add(Db.P("@n", "%" + fCNom.Text.Trim() + "%")); }
        if (fCCorreo.Text.Trim() != "") { sql.Append(" AND correo LIKE @c"); ps.Add(Db.P("@c", "%" + fCCorreo.Text.Trim() + "%")); }
        if (fCRango.Checked) { sql.Append(" AND fecha_alta BETWEEN @d1 AND @d2"); ps.Add(Db.P("@d1", fCDesde.Value.Date)); ps.Add(Db.P("@d2", fCHasta.Value.Date)); }
        sql.Append(" ORDER BY nombre");
        gC.DataSource = Db.Consulta(sql.ToString(), ps.ToArray());
    }

    void GuardarCliente(bool nuevo)
    {
        if (string.IsNullOrWhiteSpace(cNombre.Text)) { Aviso("El nombre es obligatorio."); return; }
        if (!Regex.IsMatch(cCorreo.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) { Aviso("Escribe un correo válido."); return; }
        if (string.IsNullOrWhiteSpace(cTel.Text) || cTel.Text.Trim().Length > 15) { Aviso("El teléfono es obligatorio (máx. 15 caracteres)."); return; }
        if (nuevo)
        {
            Db.Ejecutar("INSERT INTO clientes (nombre, correo, telefono, fecha_alta) VALUES (@n,@c,@t,@f)",
                Db.P("@n", cNombre.Text.Trim()), Db.P("@c", cCorreo.Text.Trim()), Db.P("@t", cTel.Text.Trim()), Db.P("@f", cFecha.Value.Date));
            Info("Cliente agregado.");
        }
        else
        {
            if (cId == 0) { Aviso("Selecciona un cliente de la tabla."); return; }
            Db.Ejecutar("UPDATE clientes SET nombre=@n, correo=@c, telefono=@t, fecha_alta=@f WHERE cliente_id=@id",
                Db.P("@n", cNombre.Text.Trim()), Db.P("@c", cCorreo.Text.Trim()), Db.P("@t", cTel.Text.Trim()), Db.P("@f", cFecha.Value.Date), Db.P("@id", cId));
            Info("Cliente modificado.");
        }
        BuscarClientes();
    }

    void EliminarCliente()
    {
        if (cId == 0) { Aviso("Selecciona un cliente de la tabla."); return; }
        if (MessageBox.Show("¿Eliminar el cliente seleccionado?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        Db.Ejecutar("DELETE FROM clientes WHERE cliente_id=@id", Db.P("@id", cId));   // si tiene reservas/pagos -> error FK controlado
        cId = 0; Info("Cliente eliminado.");
        BuscarClientes();
    }

    // ===================== RESERVAS =====================
    DataGridView gR;
    ComboBox rCli, rJuego, rEstadoNuevo, rEstadoF;
    NumericUpDown rCant;
    TextBox rClienteF;
    CheckBox rRango;
    DateTimePicker rDesde, rHasta;

    TabPage TabReservas(bool admin)
    {
        var tp = new TabPage(admin ? "Reservas" : "Registrar reserva");
        if (admin) { gR = Grid(); tp.Controls.Add(gR); }
        var fp = Flow();

        if (admin)
        {
            Sec(fp, "Filtros (vista v_reservas): cliente (LIKE), estado, rango de fecha (BETWEEN)");
            rClienteF = new TextBox();
            rEstadoF = Cmb("Todos", "Pendiente", "Completada", "Cancelada");
            rRango = new CheckBox { Text = "Usar rango de fechas", AutoSize = true };
            rDesde = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = new DateTime(2026, 10, 1) };
            rHasta = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            fp.Controls.AddRange(new Control[]
            {
                F("Cliente contiene", rClienteF, 150), F("Estado", rEstadoF, 110), F("", rRango, 150),
                F("Desde", rDesde, 110), F("Hasta", rHasta, 110),
                BP("Buscar", (s, e) => Run(BuscarReservas), 90)
            });
        }

        Sec(fp, "Nueva reserva (sp_registrar_reserva: cabecera + detalle en una transacción; el trigger valida la fecha de salida)");
        rCli = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        rJuego = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        rCant = Num(100); rCant.Minimum = 1; rCant.Value = 1;
        fp.Controls.AddRange(new Control[]
        {
            F("Cliente", rCli, 200), F("Juego", rJuego, 440), F("Cantidad", rCant, 70),
            BP("Registrar reserva", (s, e) => Run(RegistrarReserva), 170)
        });

        if (admin)
        {
            Sec(fp, "Cambiar estado de la reserva seleccionada (sp_cambiar_estado_reserva)");
            rEstadoNuevo = Cmb("Pendiente", "Completada", "Cancelada");
            fp.Controls.AddRange(new Control[]
            {
                F("Nuevo estado", rEstadoNuevo, 130),
                BP("Cambiar estado", (s, e) => Run(CambiarEstadoReserva), 130)
            });
        }
        else
        {
            Sec(fp, "Tu perfil (Operador) puede crear reservas pero no consultarlas ni modificarlas.");
        }

        tp.Controls.Add(fp);
        Action refr = () =>
        {
            CargarCombo(rCli, SqlClientesCombo);
            CargarCombo(rJuego, SqlJuegosCombo);
            if (admin) BuscarReservas();
        };
        refrescar[tp] = refr;
        Run(refr);
        return tp;
    }

    void BuscarReservas()
    {
        var sql = new StringBuilder("SELECT * FROM v_reservas WHERE 1=1");
        var ps = new List<MySql.Data.MySqlClient.MySqlParameter>();
        if (rClienteF.Text.Trim() != "") { sql.Append(" AND cliente LIKE @c"); ps.Add(Db.P("@c", "%" + rClienteF.Text.Trim() + "%")); }
        if (rEstadoF.SelectedIndex > 0) { sql.Append(" AND estado = @e"); ps.Add(Db.P("@e", rEstadoF.Text)); }
        if (rRango.Checked) { sql.Append(" AND DATE(fecha_reserva) BETWEEN @d1 AND @d2"); ps.Add(Db.P("@d1", rDesde.Value.Date)); ps.Add(Db.P("@d2", rHasta.Value.Date)); }
        sql.Append(" ORDER BY fecha_reserva DESC");
        gR.DataSource = Db.Consulta(sql.ToString(), ps.ToArray());
    }

    void RegistrarReserva()
    {
        if (rCli.SelectedValue == null || rJuego.SelectedValue == null) { Aviso("Selecciona cliente y juego."); return; }
        Db.Procedimiento("sp_registrar_reserva",
            Db.P("p_cliente_id", Convert.ToInt32(rCli.SelectedValue)),
            Db.P("p_producto_id", Convert.ToInt32(rJuego.SelectedValue)),
            Db.P("p_cantidad", (int)rCant.Value));
        Info("Reserva registrada correctamente.");
        if (Sesion.EsAdmin) BuscarReservas();
    }

    void CambiarEstadoReserva()
    {
        if (gR.CurrentRow == null) { Aviso("Selecciona una reserva de la tabla."); return; }
        int id = Convert.ToInt32(gR.CurrentRow.Cells["reserva_id"].Value);
        Db.Procedimiento("sp_cambiar_estado_reserva", Db.P("p_reserva_id", id), Db.P("p_nuevo_estado", rEstadoNuevo.Text));
        Info("Reserva #" + id + " ahora está " + rEstadoNuevo.Text + ".");
        BuscarReservas();
    }

    // ===================== VENTAS =====================
    DataGridView gV;
    ComboBox vCli, vJuego, vMetodo, vMetodoF, vEstadoF;
    NumericUpDown vCant, vMin, vMax;
    CheckBox vForzar;

    TabPage TabVentas(bool admin)
    {
        var tp = new TabPage(admin ? "Ventas / Pagos" : "Registrar venta");
        if (admin) { gV = Grid(); tp.Controls.Add(gV); }
        var fp = Flow();

        if (admin)
        {
            Sec(fp, "Filtros (vista v_ventas): método (IN), estado, monto (BETWEEN)");
            vMetodoF = Cmb("Todos", "Efectivo", "Tarjeta", "Transferencia", "Tarjeta o Transferencia");
            vEstadoF = Cmb("Todos", "Pagado", "Pendiente", "Cancelado");
            vMin = Num(1000000, 2); vMax = Num(1000000, 2); vMax.Value = 1000000;
            fp.Controls.AddRange(new Control[]
            {
                F("Método", vMetodoF, 150), F("Estado", vEstadoF, 110), F("Monto mín.", vMin, 90), F("Monto máx.", vMax, 100),
                BP("Buscar", (s, e) => Run(BuscarVentas), 90)
            });
        }

        Sec(fp, admin ? "Registrar venta" : "Registrar venta (sp_registrar_venta_directa - único medio permitido para tu perfil)");
        vCli = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        vJuego = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        vCant = Num(1000); vCant.Minimum = 1; vCant.Value = 1;
        vMetodo = Cmb("Efectivo", "Tarjeta", "Transferencia");
        fp.Controls.AddRange(new Control[]
        {
            F("Cliente", vCli, 200), F("Juego", vJuego, 440), F("Cantidad", vCant, 70), F("Método de pago", vMetodo, 150),
            BP("Vender (SP)", (s, e) => Run(VenderSP), 110),
            BP("Verificar stock (fn)", (s, e) => Run(VerificarStock), 180)
        });

        if (admin)
        {
            vForzar = new CheckBox { Text = "Forzar error (demostrar ROLLBACK)", AutoSize = true };
            fp.Controls.AddRange(new Control[]
            {
                F("", vForzar, 230),
                BP("Vender con transacción C#", (s, e) => VenderTransaccion(), 250)
            });
        }

        tp.Controls.Add(fp);
        Action refr = () =>
        {
            CargarCombo(vCli, SqlClientesCombo);
            CargarCombo(vJuego, SqlJuegosCombo);
            if (admin) BuscarVentas();
        };
        refrescar[tp] = refr;
        Run(refr);
        return tp;
    }

    void BuscarVentas()
    {
        var sql = new StringBuilder("SELECT * FROM v_ventas WHERE total BETWEEN @min AND @max");
        var ps = new List<MySql.Data.MySqlClient.MySqlParameter> { Db.P("@min", vMin.Value), Db.P("@max", vMax.Value) };
        if (vMetodoF.SelectedIndex == 4) sql.Append(" AND metodo_pago IN ('Tarjeta','Transferencia')");
        else if (vMetodoF.SelectedIndex > 0) { sql.Append(" AND metodo_pago = @m"); ps.Add(Db.P("@m", vMetodoF.Text)); }
        if (vEstadoF.SelectedIndex > 0) { sql.Append(" AND estado = @e"); ps.Add(Db.P("@e", vEstadoF.Text)); }
        sql.Append(" ORDER BY fecha_pago DESC");
        gV.DataSource = Db.Consulta(sql.ToString(), ps.ToArray());
    }

    bool DatosVentaValidos()
    {
        if (vCli.SelectedValue == null || vJuego.SelectedValue == null) { Aviso("Selecciona cliente y juego."); return false; }
        return true;
    }

    void VenderSP()
    {
        if (!DatosVentaValidos()) return;
        string msg = Db.Procedimiento("sp_registrar_venta_directa",
            Db.P("p_cliente_id", Convert.ToInt32(vCli.SelectedValue)),
            Db.P("p_producto_id", Convert.ToInt32(vJuego.SelectedValue)),
            Db.P("p_cantidad", (int)vCant.Value),
            Db.P("p_metodo_pago", vMetodo.Text),
            Db.Salida("p_mensaje", 100));
        Info(msg);   // parámetro de salida del SP
        Run(() => { CargarCombo(vJuego, SqlJuegosCombo); if (Sesion.EsAdmin) BuscarVentas(); });
    }

    void VerificarStock()
    {
        if (!DatosVentaValidos()) return;
        var r = Db.Escalar("SELECT fn_hay_stock(@p,@c)", Db.P("@p", Convert.ToInt32(vJuego.SelectedValue)), Db.P("@c", (int)vCant.Value));
        Info("fn_hay_stock devuelve: " + r);
    }

    void VenderTransaccion()
    {
        if (!DatosVentaValidos()) return;
        int prod = Convert.ToInt32(vJuego.SelectedValue);
        try
        {
            string r = Db.VentaConTransaccion(Convert.ToInt32(vCli.SelectedValue), prod, (int)vCant.Value, vMetodo.Text, vForzar.Checked);
            Info(r);
        }
        catch (Exception ex)
        {
            AppendLog("ERROR> " + ex.Message);
            MessageBox.Show("ROLLBACK ejecutado.\n\nMotivo: " + Db.Mensaje(ex) +
                "\n\nEl stock y los pagos quedaron sin cambios (atomicidad).", "Transacción revertida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        Run(() => { CargarCombo(vJuego, SqlJuegosCombo); BuscarVentas(); });
    }

    // ===================== STOCK (solo admin) =====================
    DataGridView gS;
    ComboBox sJuego;
    NumericUpDown sNuevo;

    TabPage TabStock()
    {
        var tp = new TabPage("Stock y bitácora");
        gS = Grid();
        tp.Controls.Add(gS);
        var fp = Flow();
        Sec(fp, "Actualizar stock con sp_actualizar_stock (el trigger tr_bitacora_stock registra el cambio)");
        sJuego = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        sNuevo = Num(100000);
        fp.Controls.AddRange(new Control[]
        {
            F("Juego", sJuego, 360), F("Nuevo stock", sNuevo, 90),
            BP("Actualizar stock", (s, e) => Run(() =>
            {
                if (sJuego.SelectedValue == null) { Aviso("Selecciona un juego."); return; }
                Db.Procedimiento("sp_actualizar_stock", Db.P("p_producto_id", Convert.ToInt32(sJuego.SelectedValue)), Db.P("p_nuevo_stock", (int)sNuevo.Value));
                Info("Stock actualizado. Revisa la bitácora abajo.");
                Cargar();
            }), 130)
        });
        tp.Controls.Add(fp);
        refrescar[tp] = Cargar;
        Run(Cargar);
        return tp;
    }

    void Cargar()
    {
        CargarCombo(sJuego, SqlJuegosCombo);
        gS.DataSource = Db.Consulta(
            "SELECT b.id_bitacora, j.nombre AS juego, b.stock_anterior, b.stock_nuevo, b.fecha_cambio, b.usuario " +
            "FROM bitacora_stock b INNER JOIN juegos j ON j.producto_id = b.producto_id ORDER BY b.fecha_cambio DESC");
    }

    // ===================== REPORTES (solo admin) =====================
    DataGridView gRep;
    ComboBox cmbRep;
    NumericUpDown repMin;
    TextBox txtSqlRep;

    static readonly (string Nombre, string Sql)[] Reportes =
    {
        ("Vista v_reservas (INNER JOIN x3)",
         "SELECT * FROM v_reservas ORDER BY fecha_reserva DESC"),
        ("Vista v_ventas (INNER JOIN x2)",
         "SELECT * FROM v_ventas ORDER BY fecha_pago DESC"),
        ("Vista v_inventario_marca (GROUP BY)",
         "SELECT * FROM v_inventario_marca ORDER BY valor_inventario DESC"),
        ("LEFT JOIN: juegos y veces vendidos (incluye los nunca vendidos)",
         "SELECT j.nombre, j.categoria, COUNT(p.pago_id) AS ventas, IFNULL(SUM(p.cantidad),0) AS unidades " +
         "FROM juegos j LEFT JOIN pagos p ON p.producto_id = j.producto_id AND p.estado='Pagado' " +
         "GROUP BY j.producto_id, j.nombre, j.categoria ORDER BY unidades DESC"),
        ("GROUP BY + HAVING: ingresos por categoría y mes (usa 'Monto mínimo')",
         "SELECT j.categoria, DATE_FORMAT(p.fecha_pago,'%Y-%m') AS mes, COUNT(*) AS num_ventas, SUM(p.total) AS total, " +
         "ROUND(AVG(p.total),2) AS promedio, CAST(SUM(p.cantidad) AS CHAR) AS unidades_texto " +
         "FROM pagos p INNER JOIN juegos j ON j.producto_id = p.producto_id WHERE p.estado='Pagado' " +
         "GROUP BY j.categoria, DATE_FORMAT(p.fecha_pago,'%Y-%m') HAVING SUM(p.total) >= @min ORDER BY total DESC"),
        ("Subconsulta en WHERE: juegos más caros que el promedio",
         "SELECT nombre, marca, precio_unitario FROM juegos WHERE precio_unitario > (SELECT AVG(precio_unitario) FROM juegos) ORDER BY precio_unitario DESC"),
        ("Subconsulta correlacionada: juegos >= promedio de su marca",
         "SELECT j.nombre, j.marca, j.precio_unitario FROM juegos j " +
         "WHERE j.precio_unitario >= (SELECT AVG(j2.precio_unitario) FROM juegos j2 WHERE j2.marca = j.marca) ORDER BY j.marca, j.precio_unitario DESC"),
        ("Subconsulta en FROM + función fn_ingresos_cliente",
         "SELECT t.cliente, t.reservas, fn_ingresos_cliente(t.cliente_id) AS ingresos_pagados " +
         "FROM (SELECT c.cliente_id, c.nombre AS cliente, COUNT(r.reserva_id) AS reservas FROM clientes c " +
         "LEFT JOIN reservas r ON r.cliente_id = c.cliente_id GROUP BY c.cliente_id, c.nombre) t ORDER BY ingresos_pagados DESC"),
        ("UNION (operador de conjuntos): clientes con reserva o con pago",
         "SELECT c.nombre, 'Reserva' AS origen FROM clientes c WHERE c.cliente_id IN (SELECT cliente_id FROM reservas) " +
         "UNION SELECT c.nombre, 'Pago' FROM clientes c WHERE c.cliente_id IN (SELECT cliente_id FROM pagos WHERE estado='Pagado') ORDER BY nombre"),
        ("Función fn_hay_stock: disponibilidad para 5 unidades",
         "SELECT nombre, stock, fn_hay_stock(producto_id, 5) AS disponible_para_5 FROM juegos ORDER BY stock")
    };

    TabPage TabReportes()
    {
        var tp = new TabPage("Reportes");
        gRep = Grid();
        tp.Controls.Add(gRep);
        var fp = Flow();
        Sec(fp, "Reportes con vistas, joins, subconsultas, agrupamientos, funciones y operadores de conjuntos");
        cmbRep = Cmb(Reportes.Select(r => r.Nombre).ToArray());
        repMin = Num(10000000, 2);
        txtSqlRep = new TextBox { Multiline = true, ReadOnly = true, Height = 100, ScrollBars = ScrollBars.Vertical };
        cmbRep.SelectedIndexChanged += (s, e) => txtSqlRep.Text = Reportes[cmbRep.SelectedIndex].Sql;
        txtSqlRep.Text = Reportes[0].Sql;
        fp.Controls.AddRange(new Control[]
        {
            F("Reporte", cmbRep, 520), F("Monto mínimo (HAVING)", repMin, 120),
            BP("Ejecutar", (s, e) => Run(() =>
            {
                string sql = Reportes[cmbRep.SelectedIndex].Sql;
                var ps = sql.Contains("@min") ? new[] { Db.P("@min", repMin.Value) } : new MySql.Data.MySqlClient.MySqlParameter[0];
                gRep.DataSource = Db.Consulta(sql, ps);
            }), 100)
        });
        var pSql = F("SQL del reporte", txtSqlRep, 1000);
        pSql.Height = 140;
        fp.Controls.Add(pSql);
        tp.Controls.Add(fp);
        return tp;
    }

    // ===================== PRUEBA DE PERMISOS (solo operador) =====================
    TextBox txtPermisos;

    TabPage TabPermisos()
    {
        var tp = new TabPage("Prueba de permisos");
        txtPermisos = new TextBox
        {
            Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 11F), BackColor = Color.White
        };
        tp.Controls.Add(txtPermisos);

        var fp = Flow();
        Sec(fp, "Perfil OPERADOR (rol_operador) - PERMITIDO: consultar juegos y clientes, registrar ventas (SP), registrar reservas (SP), verificar stock (fn).");
        Sec(fp, "NO PERMITIDO: modificar/eliminar registros, ver pagos o reservas, cambiar el stock directamente. Prueba cada operación:");
        fp.Controls.AddRange(new Control[]
        {
            BP("DELETE en juegos", (s, e) => Probar("DELETE FROM juegos", false, () =>
                Db.Ejecutar("DELETE FROM juegos WHERE producto_id = @id", Db.P("@id", -1))), 230),
            BP("UPDATE de stock directo", (s, e) => Probar("UPDATE juegos (stock)", false, () =>
                Db.Ejecutar("UPDATE juegos SET stock = stock + 1 WHERE producto_id = @id", Db.P("@id", -1))), 250),
            BP("SELECT en pagos", (s, e) => Probar("SELECT * FROM pagos", false, () =>
                Db.Consulta("SELECT * FROM pagos LIMIT 5")), 230),
            BP("SELECT en reservas", (s, e) => Probar("SELECT * FROM reservas", false, () =>
                Db.Consulta("SELECT * FROM reservas LIMIT 5")), 230),
            BP("INSERT directo en pagos", (s, e) => Probar("INSERT INTO pagos", false, InsertPagoPrueba), 270),
            BP("SELECT en juegos (permitido)", (s, e) => Probar("SELECT * FROM juegos", true, () =>
                Db.Consulta("SELECT producto_id FROM juegos LIMIT 5")), 290),
            BP("fn_hay_stock (permitido)", (s, e) => Probar("fn_hay_stock", true, () =>
                Db.Escalar("SELECT fn_hay_stock(@p, 1)", Db.P("@p", 1))), 260)
        });
        tp.Controls.Add(fp);
        txtPermisos.Text = "Los resultados de cada prueba aparecen aquí." + Environment.NewLine;
        return tp;
    }

    /// <summary>INSERT de prueba dentro de una transacción que SIEMPRE se revierte (no deja datos).</summary>
    void InsertPagoPrueba()
    {
        using var c = Db.Abrir();
        using var tx = c.BeginTransaction();
        try
        {
            using var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                "INSERT INTO pagos (cliente_id, producto_id, cantidad, total, metodo_pago, estado) VALUES (@c,@p,1,1,'Efectivo','Pagado')", c, tx);
            cmd.Parameters.Add(Db.P("@c", 1)); cmd.Parameters.Add(Db.P("@p", 1));
            cmd.ExecuteNonQuery();
        }
        finally { tx.Rollback(); }
    }

    /// <summary>Ejecuta una operación y muestra siempre el resultado: permitida o denegada.</summary>
    void Probar(string operacion, bool deberiaPermitirse, Action accion)
    {
        string resultado;
        MessageBoxIcon icono;
        try
        {
            accion();
            resultado = deberiaPermitirse
                ? "PERMITIDO (esperado): tu perfil puede ejecutar " + operacion + "."
                : "SE EJECUTÓ: tu perfil SÍ tiene permiso para " + operacion + ". (Si no debería, revisa los GRANT/REVOKE del rol; el INSERT de prueba se revirtió.)";
            icono = deberiaPermitirse ? MessageBoxIcon.Information : MessageBoxIcon.Warning;
        }
        catch (Exception ex)
        {
            AppendLog("ERROR> " + ex.Message);
            resultado = (deberiaPermitirse ? "ERROR INESPERADO en " : "DENEGADO (esperado) en ") + operacion + ": " + Db.Mensaje(ex);
            icono = deberiaPermitirse ? MessageBoxIcon.Error : MessageBoxIcon.Stop;
        }
        txtPermisos.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + resultado + Environment.NewLine + Environment.NewLine);
        MessageBox.Show(resultado, "Prueba de permisos", MessageBoxButtons.OK, icono);
    }
}
