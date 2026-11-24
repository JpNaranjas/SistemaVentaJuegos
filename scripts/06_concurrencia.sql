use tienda_juegos;

-- 4.5 Rollback forzado
start transaction;
update juegos set stock = stock - 1 where producto_id = 1;
insert into pagos (cliente_id, producto_id, cantidad, total, metodo_pago, estado)
values (1, 1, 1, 1600.00, 'Efectivo', 'Pagado');
rollback;

-- 4.5 Commit exitoso
start transaction;
update juegos set stock = stock - 1 where producto_id = 2;
insert into pagos (cliente_id, producto_id, cantidad, total, metodo_pago, estado)
values (2, 2, 1, 860.00, 'Efectivo', 'Pagado');
commit;

-- 4.4 Nivel de aislamiento: READ COMMITTED (Lectura no repetible)
-- NOTA: Ejecutar en dos pestañas de Workbench
/*
-- Sesión 1
set session transaction isolation level read committed;
start transaction;
select stock from juegos where producto_id = 1;

-- Sesión 2
set session transaction isolation level read committed;
start transaction;
update juegos set stock = 10 where producto_id = 1;
commit;

-- Sesión 1 (Vuelve a consultar)
select stock from juegos where producto_id = 1;
commit;
*/

-- 4.4 Nivel de aislamiento: SERIALIZABLE (Bloqueo)
/*
-- Sesión 1
set session transaction isolation level serializable;
start transaction;
select * from juegos where producto_id = 1;

-- Sesión 2 (Se quedará bloqueada)
set session transaction isolation level serializable;
start transaction;
update juegos set stock = 5 where producto_id = 1;
*/