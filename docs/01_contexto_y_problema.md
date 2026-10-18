# 1. Descripción y contexto del problema

## 1.1 Contexto

La organización del caso es una **distribuidora de insumos médicos** de tamaño pequeño. Compra a proveedores nacionales insumos clínicos (guantes, gasas, jeringas) y medicamentos de uso general, los almacena en una bodega y los despacha a clínicas, centros médicos y farmacias.

Hoy la bodega se controla con **planillas Excel** que mantienen varias personas. Cada planilla mezcla en una sola hoja los datos del producto, del proveedor y de los movimientos de entrada y salida.

## 1.2 Problemática

| # | Problema observado | Consecuencia para el negocio |
|---|---|---|
| P1 | El stock se escribe a mano y cada persona actualiza su propia copia de la planilla. | El stock informado no coincide con el físico, lo que provoca quiebres de stock y despachos que no se pueden cumplir. |
| P2 | No queda registro de quién cambió el stock, cuándo ni por qué. | No se pueden explicar las diferencias de inventario ni detectar errores o pérdidas. |
| P3 | Los datos del proveedor y la categoría se repiten en cada fila. | Aparecen inconsistencias: un mismo proveedor con distintos RUT o razones sociales. |
| P4 | No se controla el stock mínimo ni la fecha de vencimiento. | Se compra tarde y se despachan o pierden productos vencidos, lo que es crítico en insumos de salud. |
| P5 | Las planillas se comparten por correo y no tienen control de acceso. | Cualquiera puede modificar o borrar información, y existe riesgo de fuga de datos comerciales (precios y proveedores). |
| P6 | Se pueden ingresar SKU duplicados, precios negativos o RUT inválidos. | Los reportes salen con datos erróneos. |

**Planteamiento formal:** la distribuidora necesita un sistema que centralice el maestro de productos, categorías y proveedores y que controle el stock **solo mediante movimientos registrados y trazables**. Ese control debe garantizar la integridad, la confidencialidad y la disponibilidad de la información, según los controles de la norma ISO/IEC 27001.

## 1.3 Solución propuesta: SIGBIM

**SIGBIM** (Sistema de Gestión de Bodega de Insumos Médicos) es una aplicación de escritorio en C# (.NET) con base de datos MySQL. Ofrece:

1. Mantenedores CRUD de **Productos**, **Categorías** y **Proveedores**.
2. Registro de **movimientos de stock** (ENTRADA, SALIDA y AJUSTE). Es la única forma de modificar el stock y deja historial (kardex).
3. Una **vista consolidada** (`vw_productodetalle`) que reúne en una sola consulta el producto, su categoría, su proveedor, el estado del stock, el valor del inventario y el último movimiento.
4. Seguridad aplicada: usuario de base de datos con privilegio mínimo, operaciones solo mediante procedimientos almacenados, auditoría de cada cambio y bitácora de eventos.

## 1.4 Objetivos

**Objetivo general:** desarrollar un mantenedor de inventario en arquitectura de 4 capas que asegure la integridad y la trazabilidad del stock de insumos médicos.

**Objetivos específicos:**
- OE1: Modelar los requerimientos funcionales y no funcionales con la tríada de requerimientos y controles ISO/IEC 27001.
- OE2: Diseñar una base de datos normalizada (3FN) con integridad referencial y procedimientos almacenados para todas las operaciones.
- OE3: Implementar la solución en capas (UI, BLL, DAL y Entidades) con validaciones y manejo de errores en cada capa.
- OE4: Versionar todo el desarrollo en GitHub de forma incremental.

## 1.5 Alcance

| Incluye | No incluye (fuera de alcance) |
|---|---|
| CRUD de productos, categorías y proveedores | Ventas, facturación y cotizaciones |
| Entradas, salidas y ajustes de stock con historial | Gestión de usuarios y login propio (se usa el usuario del sistema operativo para la auditoría) |
| Alertas de stock bajo el mínimo (estado en la vista) | Integración con ERP o con el SII |
| Auditoría de creación y modificación, y bitácora de la aplicación | Múltiples bodegas o sucursales |
| Ejecución en macOS (también Windows y Linux) | Aplicación web o móvil |

## 1.6 Actores

| Actor | Descripción |
|---|---|
| **Encargado de bodega** | Registra entradas y salidas de stock y consulta productos y kardex. |
| **Administrador de inventario** | Hereda lo que hace el encargado y además administra el maestro de productos, categorías y proveedores. |
| **Base de datos MySQL** (actor secundario) | Ejecuta los procedimientos almacenados y aplica las reglas de integridad. |

## 1.7 Glosario

- **SKU:** código único interno del producto.
- **Kardex:** historial cronológico de movimientos de un producto con su stock anterior y nuevo.
- **Baja lógica:** marcar un registro como inactivo (`Activo = 0`) en vez de borrarlo, para conservar el historial.
- **SP:** procedimiento almacenado (stored procedure).
- **Concurrencia optimista:** técnica que detecta si otro usuario modificó el registro (campo `Version`) antes de guardar.
