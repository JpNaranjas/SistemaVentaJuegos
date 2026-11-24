use tienda_juegos;

-- ==========================================
-- TRIGGERS
-- ==========================================
delimiter //
create trigger tr_validar_fecha_reserva
before insert on detalle_reserva
for each row
begin
    declare v_fecha_salida date;
    
    select fecha_salida into v_fecha_salida 
    from juegos 
    where producto_id = new.producto_id;
    
    if v_fecha_salida is not null then
        if current_date() < date_sub(v_fecha_salida, interval 1 month) then
            signal sqlstate '45000' 
            set message_text = 'Error: Solo se puede reservar con un máximo de 1 mes de anticipación a la fecha de salida.';
        end if;
    end if;
end;
//
delimiter ;

delimiter //
create trigger tr_bitacora_stock
after update on juegos
for each row
begin
    if old.stock <> new.stock then
        insert into bitacora_stock (producto_id, stock_anterior, stock_nuevo, usuario)
        values (old.producto_id, old.stock, new.stock, current_user());
    end if;
end;
//
delimiter ;

-- ==========================================
-- STORED PROCEDURES
-- ==========================================
delimiter //
create procedure sp_registrar_venta_directa(
    in p_cliente_id int,
    in p_producto_id int,
    in p_cantidad int,
    in p_metodo_pago varchar(20),
    out p_mensaje varchar(100)
)
begin
    declare v_precio decimal(10,2);
    declare v_stock int;
    declare v_total decimal(10,2);
    
    declare exit handler for sqlexception
    begin
        rollback;
        set p_mensaje = 'Error: Ocurrió un problema al registrar la venta.';
    end;

    start transaction;
    
    select precio_unitario, stock into v_precio, v_stock 
    from juegos where producto_id = p_producto_id for update;
    
    if v_stock >= p_cantidad then
        set v_total = v_precio * p_cantidad;
        
        insert into pagos (cliente_id, producto_id, cantidad, total, metodo_pago, estado)
        values (p_cliente_id, p_producto_id, p_cantidad, v_total, p_metodo_pago, 'Pagado');
        
        update juegos set stock = stock - p_cantidad where producto_id = p_producto_id;
        
        commit;
        set p_mensaje = concat('Venta exitosa. Total: $', v_total);
    else
        rollback;
        set p_mensaje = 'Error: No hay stock suficiente.';
    end if;
end;
//
delimiter ;

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

delimiter //
create procedure sp_actualizar_stock(
    in p_producto_id int,
    in p_nuevo_stock int
)
begin
    update juegos set stock = p_nuevo_stock where producto_id = p_producto_id;
end;
//
delimiter ;

delimiter //
create procedure sp_cambiar_estado_reserva(
    in p_reserva_id int,
    in p_nuevo_estado varchar(20)
)
begin
    update reservas set estado = p_nuevo_estado where reserva_id = p_reserva_id;
end;
//
delimiter ;

-- ==========================================
-- FUNCIONES
-- ==========================================
delimiter //
create function fn_ingresos_cliente(p_cliente_id int) 
returns decimal(10,2)
deterministic
begin
    declare v_total decimal(10,2);
    select ifnull(sum(total), 0.00) into v_total 
    from pagos 
    where cliente_id = p_cliente_id and estado = 'Pagado';
    return v_total;
end;
//
delimiter ;

delimiter //
create function fn_hay_stock(p_producto_id int, p_cantidad int) 
returns varchar(20)
deterministic
begin
    declare v_stock int;
    select stock into v_stock from juegos where producto_id = p_producto_id;
    
    if v_stock >= p_cantidad then
        return 'Disponible';
    else
        return 'Agotado';
    end if;
end;
//
delimiter ;

-- ==========================================
-- VISTAS (2.6)
-- ==========================================
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