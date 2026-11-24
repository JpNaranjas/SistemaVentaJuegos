-- ==========================================================
-- 07_app_vistas_y_permisos.sql
-- Ejecutar DESPUÉS de tu script completo (con un usuario root/DBA)
-- ==========================================================
use tienda_juegos;

-- ---------- 2.6 VISTAS usadas por la app ----------
create or replace view v_reservas as
select r.reserva_id, c.nombre as cliente, j.nombre as juego, d.cantidad,
       d.precio_unitario, r.total, r.estado, r.fecha_reserva
from reservas r
inner join clientes c on c.cliente_id = r.cliente_id
inner join detalle_reserva d on d.reserva_id = r.reserva_id
inner join juegos j on j.producto_id = d.producto_id;

create or replace view v_ventas as
select p.pago_id, c.nombre as cliente, j.nombre as juego, p.cantidad,
       p.total, p.metodo_pago, p.estado, p.fecha_pago
from pagos p
inner join clientes c on c.cliente_id = p.cliente_id
inner join juegos j on j.producto_id = p.producto_id;

create or replace view v_inventario_marca as
select marca, count(*) as juegos, sum(stock) as unidades,
       sum(stock * precio_unitario) as valor_inventario
from juegos
group by marca;

-- ---------- Corrección: sp_registrar_reserva debe RE-LANZAR el error ----------
-- (si no, el mensaje del trigger de fecha nunca llega a la aplicación)
drop procedure if exists sp_registrar_reserva;
delimiter //
create procedure sp_registrar_reserva(
    in p_cliente_id int,
    in p_producto_id int,
    in p_cantidad int
)
begin
    declare v_precio decimal(10,2);
    declare v_reserva_id int;

    declare exit handler for sqlexception
    begin
        rollback;
        resignal;
    end;

    start transaction;
    select precio_unitario into v_precio from juegos where producto_id = p_producto_id;

    insert into reservas (cliente_id, estado, total)
    values (p_cliente_id, 'Pendiente', (v_precio * p_cantidad));
    set v_reserva_id = last_insert_id();

    insert into detalle_reserva (reserva_id, producto_id, cantidad, precio_unitario)
    values (v_reserva_id, p_producto_id, p_cantidad, v_precio);

    commit;
end;
//
delimiter ;

-- ---------- 3.3 Privilegios extra para el perfil OPERADOR ----------
grant execute on procedure tienda_juegos.sp_registrar_reserva to 'rol_operador';
grant execute on function  tienda_juegos.fn_hay_stock         to 'rol_operador';

-- Roles activos por defecto al iniciar sesión
set default role 'rol_administrador' to 'admin_tienda'@'localhost';
set default role 'rol_operador'      to 'operador_tienda'@'localhost';

-- Ejemplo de REVOKE (3.3): el operador no debe poder insertar pagos directo, solo por SP
revoke insert on tienda_juegos.pagos from 'rol_operador';
