# Aura Gaming – Sistema de tienda de videojuegos y juegos de mesa

Aplicación de escritorio en **C# (Windows Forms, .NET 8)** conectada a **MySQL 8** mediante **ADO.NET (MySql.Data)**.
Todo el SQL está escrito a mano y parametrizado (sin Entity Framework ni Dapper).

Proyecto final – Taller de Base de Datos.

---

## 1. Requisitos

| Software | Versión | Para qué |
|---|---|---|
| MySQL Server | 8.0 o superior (los roles requieren 8.0+) | Base de datos |
| MySQL Workbench (opcional) | Cualquiera | Ejecutar los scripts |
| Visual Studio 2022 | Con la carga de trabajo "Desarrollo de escritorio con .NET" | Compilar y ejecutar |
| .NET SDK | 8.0 o superior | Incluido con Visual Studio |
| Conexión a internet (solo la primera vez) | — | Restaurar paquetes NuGet |

---

## 2. Crear la base de datos

> **Advertencia:** el script principal comienza con `DROP DATABASE IF EXISTS tienda_juegos;`.
> Si ya existe una base con ese nombre, **se borrará**.

1. Verifica que el servicio de MySQL esté activo (Windows: `services.msc` → *MySQL80* → En ejecución). Puerto por defecto: **3306**.
2. Abre MySQL Workbench y conéctate con un usuario administrador (por ejemplo `root`). Este es el **único** momento en que se usa root: solo para crear la base y los usuarios; la aplicación nunca se conecta como root.
3. Ejecuta los scripts SQL en el **siguiente orden estricto**. Puedes abrirlos desde la carpeta `/sql` y ejecutarlos uno por uno:
   *   `01_esquema.sql` (Crea la base de datos, tablas y restricciones).
   *   `02_datos.sql` (Inserta los 15 registros de prueba por tabla).
   *   `04_procedural.sql` (Crea triggers, procedimientos, funciones y vistas).
   *   `03_seguridad.sql` (Crea los roles y usuarios de la base de datos).
*Nota:* Al finalizar, tendrás la base de datos `tienda_juegos` lista, con usuarios como `VaneRoll` (Admin) y `JpNaranjas` (Operador).

4. Verifica que todo se creó correctamente:

   ```sql
   USE tienda_juegos;
   SHOW TABLES;                                  -- 5 tablas + bitacora_stock
   SELECT COUNT(*) FROM juegos;                  -- 15 o más
   SHOW FULL TABLES WHERE Table_type = 'VIEW';   -- 3 vistas
   SHOW PROCEDURE STATUS WHERE Db = 'tienda_juegos';
   SHOW FUNCTION STATUS  WHERE Db = 'tienda_juegos';
   SHOW TRIGGERS;                                -- 2 triggers
   ```

### Usuarios de base de datos

| Perfil | Usuario | Contraseña | Rol |
|---|---|---|---|
| Administrador | `admin_tienda` | `Admin123!` | `rol_administrador` |
| Operador | `operador_tienda` | `Oper123!` | `rol_operador` |
| Administrador | `VaneRoll` | `All2well` | `rol_administrador` |
| Operador | `JpNaranjas` | `ViceCity` | `rol_operador` |

> Los nombres de usuario distinguen mayúsculas y minúsculas (`VaneRoll` ≠ `vaneroll`).
> Estas contraseñas son solo para el proyecto escolar.

---

## 3. Ejecutar la aplicación

1. Abre `TiendaJuegos.csproj` (o la solución) con Visual Studio 2022.
2. Espera a que se restauren los paquetes NuGet: `MySql.Data` y `System.Configuration.ConfigurationManager`.
   Si no se restauran solos: clic derecho en la solución → **Restaurar paquetes NuGet**.
3. Revisa `App.config`. Si tu MySQL no está en `localhost:3306`, cambia el servidor o el puerto:

   ```xml
   <add name="TiendaJuegos"
        connectionString="Server=localhost;Port=3306;Database=tienda_juegos;AllowPublicKeyRetrieval=True;CheckParameters=False;AllowUserVariables=True;" />
   ```

   - El usuario y la contraseña **no** van aquí: se toman de la pantalla de inicio de sesión.
   - `CheckParameters=False` es necesario para que el perfil Operador pueda llamar a los procedimientos almacenados sin acceso a las tablas del sistema.
4. Verifica que la carpeta `Imagenes/` (mario.jpeg, uno.jpeg, gta6.png) se copie junto al ejecutable. Ya está configurado en el `.csproj`; si faltara una imagen, la app abre igual sin ella.
5. Presiona **F5** (Iniciar depuración).

### Uso básico

1. En la pantalla de inicio escribe el usuario y la contraseña de BD y presiona **el botón del perfil**:
   - *Iniciar sesión como ADMINISTRADOR* (requiere el rol `rol_administrador`).
   - *Iniciar sesión como OPERADOR* (requiere el rol `rol_operador`).
2. La barra superior muestra **AURA GAMING**, el nombre del usuario y la cuenta con la que se inició sesión.
3. El menú por pestañas cambia según el perfil:

   | Pestaña | Administrador | Operador |
   |---|---|---|
   | Juegos | CRUD + filtros | Catálogo con filtros |
   | Clientes | CRUD + filtros | Solo consulta |
   | Reservas | Listado, filtros, registrar, cambiar estado | Solo registrar |
   | Ventas / Pagos | Listado, filtros, vender (SP), vender con transacción C#, verificar stock | Vender (SP) y verificar stock |
   | Stock y bitácora | Actualizar stock y ver bitácora | — |
   | Reportes | 10 reportes (vistas, JOIN, subconsultas, GROUP BY, UNION…) | — |
   | Prueba de permisos | — | Pruebas de operaciones permitidas y denegadas |

4. La consola de la parte inferior muestra cada sentencia SQL ejecutada con sus parámetros.
5. **Cerrar sesión** regresa a la pantalla de inicio.

---

## 4. Estructura del proyecto

```
TiendaJuegos/
├── TiendaJuegos.csproj
├── App.config              Cadena de conexión (sin usuario ni contraseña)
├── Program.cs              Punto de entrada
├── Db.cs                   Capa de acceso a datos (DAO) y clase Sesion
├── Form1.cs                Inicio de sesión
├── Form2.cs                Menú principal y pantallas
├── Recursos.cs             Carga de imágenes
├── Imagenes/               mario.jpeg, uno.jpeg, gta6.png
├── sql/                    Scripts de la base de datos
└── README.md
```

---

## 5. Solución de problemas

| Síntoma | Causa probable | Solución |
|---|---|---|
| "Usuario o contraseña incorrectos" | Credenciales mal escritas (se distingue mayúsculas) | Revisa el usuario y la contraseña; confirma con `SELECT user, host FROM mysql.user;` |
| "No se pudo conectar al servidor MySQL" | Servicio detenido o puerto distinto | Inicia el servicio MySQL80 y revisa `App.config` |
| "El usuario no tiene el rol…" | Perfil equivocado o rol no asignado | Usa el botón del perfil correcto; ejecuta `SHOW GRANTS FOR 'usuario'@'localhost';` |
| "Procedure or function … cannot be found" (operador) | Falta `CheckParameters=False` en la cadena de conexión | Revisa `App.config` |
| "PERMISO DENEGADO" al registrar una reserva | Falta el `GRANT EXECUTE` | Ejecuta `sql/07_app_vistas_y_permisos.sql` |
| Error de roles al crear usuarios | MySQL anterior a 8.0 | Actualiza a MySQL 8.0 o superior |
| Error al restaurar paquetes | Sin internet o NuGet desactualizado | Verifica la conexión y restaura los paquetes |
| La app abre sin imágenes | Falta la carpeta `Imagenes/` junto al .exe | Revisa que existan los 3 archivos y que se copien al compilar |

