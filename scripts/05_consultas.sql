use tienda_juegos;

-- 2.2 Consultas con WHERE y LIKE
select * from juegos where marca like '%Nintendo%';
select * from juegos where marca like '%Hasbro%';

-- 2.3 Los 3 productos más caros
select nombre, categoria, marca, precio_unitario, stock 
from juegos order by precio_unitario desc limit 3;

-- 2.4 INNER JOIN
select p.pago_id, c.nombre as cliente, j.nombre as producto, p.cantidad, p.total, p.fecha_pago
from pagos p
inner join clientes c on p.cliente_id = c.cliente_id
inner join juegos j on p.producto_id = j.producto_id;

-- 2.4 LEFT JOIN
select j.nombre as juego, dr.reserva_id
from juegos j
left join detalle_reserva dr on j.producto_id = dr.producto_id;

-- 2.3 Reporte con GROUP BY, HAVING, CAST
select j.categoria, 
       count(*) as total_ventas, 
       sum(p.total) as ingresos_totales,
       cast(avg(p.total) as decimal(10,2)) as promedio_venta
from pagos p
inner join juegos j on p.producto_id = j.producto_id
group by j.categoria
having sum(p.total) > 1000
order by ingresos_totales desc;

-- 2.5 Subconsultas
select nombre, precio_unitario 
from juegos 
where precio_unitario > (select avg(precio_unitario) from juegos);