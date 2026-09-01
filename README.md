# K-et POS

Sistema de punto de venta de escritorio para **ABARROTES EL ESTUDIANTE**
(La Paz, B.C.S.), una tiendita de abarrotes. Cubre el flujo típico del
negocio: cobro en caja con lector de código de barras, inventario, clientes
con fiado/crédito, proveedores, usuarios con roles, apertura/cierre de caja
por turno, historial de ventas con devoluciones, y alta de productos nuevos
desde el celular escaneando el código de barras con la cámara.

> ## ⚠️ Estado: proyecto NO terminado, congelado hasta enero 2027
>
> Este es un proyecto personal en desarrollo, no un producto terminado. Se
> congeló el 2026-08-20 para enfocar el tiempo en la escuela y en otro
> proyecto (una app de finanzas). No es abandono: tiene fecha de retoma
> escrita. La sección [Qué falta](#qué-falta-antes-de-poder-usarse-en-la-tienda-real)
> de este documento explica exactamente qué le falta antes de poder correr
> en la caja real de la tienda.

## Qué hace hoy (funcional de punta a punta)

- Login con bloqueo por intentos fallidos y contraseñas con hash BCrypt.
- Cobro con escáner de código de barras USB (modo teclado/HID), carrito con
  3 pestañas de venta simultánea, búsqueda de producto, venta a
  Efectivo / Tarjeta / Autoconsumo / Fiado, guardado transaccional.
- Inventario: alta/edición vía upsert, paginación, búsqueda full-text.
- Clientes con fiado (crédito y saldo pendiente) y proveedores (CRUD básico).
- Historial de ventas con devoluciones (transaccional, respeta cantidades ya
  devueltas).
- API + página web para dar de alta productos desde el celular escaneando
  con la cámara (flujo completo: el POST llega, valida y hace upsert en la
  misma base de datos que usa la app de escritorio).

## Arquitectura

Tres piezas que comparten una sola base de datos MySQL, sin comunicarse
directamente entre sí:

| Componente | Qué es | Está en este repo |
|---|---|---|
| `POS_Presentacion` (+ `POS_Logica`, `POS_Datos`, `POS_Entidades`) | App de escritorio WPF — el POS real que usa el cajero | ✅ Sí |
| `POS_API` | API mínima (ASP.NET Core) con un endpoint, `POST /api/productos/registrar`, para dar de alta productos desde el celular | ✅ Sí |
| Página móvil (`PuntoDeVenta/index.html`) | HTML suelto con escáner de cámara (Bootstrap + html5-qrcode) que le pega a la API por HTTP en la misma red Wi-Fi | ❌ No — vive fuera de esta carpeta, todavía sin versionar |
| `mapa_base_datos.sql` | Dump/snapshot del esquema MySQL | ❌ No — vive fuera de esta carpeta, todavía sin versionar |

La app WPF es el POS "de verdad"; la API + la página móvil son un atajo
para no tener que dar de alta productos desde el teclado de la PC.

### Capas de la app de escritorio (N-capas clásico)

```
POS_Presentacion  →  POS_Logica  →  POS_Datos  →  MySQL
   (WPF/XAML)        (reglas de       (SQL crudo,
                       negocio)        sin ORM)

POS_Entidades: modelos puros (POCOs) usados por las tres capas
```

## Stack técnico

- **App de escritorio**: WPF sobre .NET Framework 4.7.2 (proyecto clásico,
  `packages.config`). Material Design in XAML.
- **API**: ASP.NET Core Web API sobre .NET 8, con Swagger.
- **Base de datos**: MySQL 8.0, charset `utf8mb4`, engine InnoDB.
- **Acceso a datos**: `MySql.Data` en la app de escritorio, `MySqlConnector`
  en la API — dos drivers distintos para la misma base (inconsistencia
  conocida, no resuelta).
- **Seguridad de contraseñas de usuarios del sistema**: `BCrypt.Net-Next`.
- **Hardware serie**: `System.IO.Ports` (báscula).

## Hardware integrado

**Ninguno de los tres periféricos está conectado a hardware físico
todavía — todo corre en modo simulador.**

- **Impresora térmica POS-58**: arma el ticket como texto plano. En modo
  simulador (activo hoy) lo guarda en un `.txt` y lo abre en el Bloc de
  notas en vez de mandarlo por USB. El envío real ESC/POS está sin escribir.
  Además, solo se llama desde el historial (reimpresión) — el cobro normal
  **no imprime nada automáticamente** hoy.
- **Cajón de dinero (RJ11)**: no tiene código propio; depende de que la
  impresora térmica esté funcionando de verdad (el cajón se abre con un
  comando ESC/POS que la impresora reenvía). Nunca se ha probado con
  hardware real.
- **Escáner de código de barras USB**: **esta parte sí funciona.** No
  requiere driver propio porque el escáner actúa como teclado (HID). El
  `TextBox` de la pantalla de ventas captura lo que "teclea" el escáner y
  dispara la búsqueda del producto al detectar Enter.
- **Báscula (puerto serie, COM3)**: mismo patrón que la impresora —
  simulador activo, código real de `SerialPort` ya escrito pero sin probar
  contra hardware.

## Qué falta antes de poder usarse en la tienda real

- **Impresión de tickets real** — hoy no hay envío de bytes a la impresora
  térmica; el cobro normal ni siquiera genera un ticket.
- **Apertura de cajón de dinero** — depende de que la impresora funcione de
  verdad; sin probar con hardware.
- **Corte de caja** — `CajaLogica.CalcularTotalesDelTurno` está hardcodeado
  para devolver `0`. Se puede abrir y cerrar turno, pero el sistema **no
  calcula** cuánto debería haber en caja contra las ventas reales; el
  cajero se fía de su propio conteo.
- **Movimientos de caja** (ingresos extra, retiros, pagos a proveedor) —
  `CajaLogica.ProcesarMovimiento` tiene los `if/else` vacíos, y ni siquiera
  existe la tabla en la base de datos para guardarlos.
- **Báscula** — sin probar con hardware real.
- **Autenticación en la API** — el endpoint `POST /api/productos/registrar`
  no tiene ningún esquema de autenticación ni autorización, y el CORS está
  abierto a cualquier origen (`AllowAnyOrigin/Header/Method`). Cualquier
  dispositivo conectado a la misma red Wi-Fi de la tienda puede llamarlo sin
  credenciales y dar de alta productos falsos en el inventario. El riesgo
  práctico es bajo mientras la API solo corra en la red local de la tienda,
  pero es un hueco real pendiente de cerrar (por ejemplo con una API key
  fija por header) antes de considerar el proyecto listo para producción.
- **Inconsistencia de driver MySQL** entre la app de escritorio
  (`MySql.Data`) y la API (`MySqlConnector`) — funciona, pero no está
  unificado.
- **Limpieza pendiente**: `POS_API/WeatherForecast.cs` es el archivo de
  ejemplo por default de `dotnet new webapi`, sin usar.
- **La página móvil y el dump de la base de datos** (`PuntoDeVenta/`,
  `mapa_base_datos.sql`) todavía no están versionados en este repositorio.
- **`IP_COMPUTADORA` hardcodeada** en la página móvil — si cambia la IP
  local de la PC hay que editarla a mano ahí.

## Esquema de base de datos (resumen)

9 tablas: `usuarios`, `roles`, `productos` (PK = código de barras),
`proveedores`, `clientes`, `ventas`, `ventas_detalles`, `devoluciones`,
`caja_turnos`. El modelo `CajaFlujo` existe en código pero **no tiene tabla
correspondiente** — es un diseño pensado para los movimientos de caja que
todavía no se persiste.

## Cómo correrlo desde cero

**Requisitos:**

- Windows con Visual Studio 2022 (workloads *.NET desktop development* y
  *ASP.NET and web development*, para .NET Framework 4.7.2 y .NET 8 SDK).
- MySQL Server 8.x corriendo localmente o accesible en red.

**Pasos:**

1. Crear la base de datos importando el dump del esquema (`mapa_base_datos.sql`,
   fuera de este repo por ahora).
2. Copiar `POS_Presentacion/ConnectionStrings.config.example` a
   `POS_Presentacion/ConnectionStrings.config` (este archivo está en
   `.gitignore` a propósito — nunca subirlo con la contraseña real) y poner
   ahí tu cadena de conexión real.
3. La tabla `usuarios` queda vacía tras importar el `.sql`. Hay que insertar
   un usuario admin manualmente (`UsuarioLogica.ObtenerHashTemporal()` genera
   el hash BCrypt de una contraseña de prueba).
4. Restaurar paquetes NuGet de `SistemaPuntoDeVenta.slnx` (Visual Studio lo
   hace solo al abrir la solución).
5. Levantar la API: desde `POS_API/`, `dotnet run` (o F5 en Visual Studio
   con `POS_API` como proyecto de inicio). Escucha en `http://0.0.0.0:5193`,
   Swagger en `/swagger`.
6. Levantar la app de escritorio: abrir la solución en Visual Studio, poner
   `POS_Presentacion` como proyecto de inicio, F5. Solo necesita la API si
   se va a usar el alta de productos desde el celular — el resto habla
   directo a MySQL.

## Seguridad

- La cadena de conexión con la contraseña real de MySQL **nunca se sube** —
  vive en `POS_Presentacion/ConnectionStrings.config`, que está en
  `.gitignore`. Este repo solo trae `ConnectionStrings.config.example` con
  un placeholder.
- Ver el hueco de autenticación de la API en la sección
  [Qué falta](#qué-falta-antes-de-poder-usarse-en-la-tienda-real).

## Roadmap

Este proyecto es el **Movimiento 2** de un plan de largo plazo: cerrar los
pendientes de arriba, terminar el rediseño, e instalarlo con usuario real en
la tienda de abarrotes. Se retoma en **enero 2027**.
