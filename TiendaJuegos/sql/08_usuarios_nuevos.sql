-- Usuarios adicionales (ejecutar con root)
create user if not exists 'VaneRoll'@'localhost'   identified by 'All2well';
create user if not exists 'JpNaranjas'@'localhost' identified by 'ViceCity';

grant 'rol_administrador' to 'VaneRoll'@'localhost';
grant 'rol_operador'      to 'JpNaranjas'@'localhost';

-- Activa el rol automáticamente al iniciar sesión
set default role 'rol_administrador' to 'VaneRoll'@'localhost';
set default role 'rol_operador'      to 'JpNaranjas'@'localhost';

-- Verificación: debe mostrar el GRANT del rol y el rol por defecto
show grants for 'VaneRoll'@'localhost';
show grants for 'JpNaranjas'@'localhost';
select user, host from mysql.user where user in ('VaneRoll','JpNaranjas');
select * from mysql.default_roles;
