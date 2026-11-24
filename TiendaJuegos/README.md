# Tienda de Juegos – App C# (WinForms, .NET 8, MySQL)

## Pasos
1. En MySQL 8 ejecuta con root: tu script completo (esquema, datos, procedural, seguridad) y después `sql/07_app_vistas_y_permisos.sql`.
2. Abre `TiendaJuegos.csproj` en Visual Studio 2022 (restaura NuGet: MySql.Data y System.Configuration.ConfigurationManager).
3. Ajusta servidor/puerto en `App.config` si es necesario.
4. Ejecuta (F5). Usuarios:
   - Administrador: `admin_tienda` / `Admin123!`
   - Operador: `operador_tienda` / `Oper123!`

## Estructura
- `Db.cs` – capa DAO (conexión, consultas parametrizadas, SPs, transacción, mensajes de error). Sin ORM.
- `Form1.cs` – login con un botón por perfil.
- `Form2.cs` – menú por pestañas, cambia según el perfil.
