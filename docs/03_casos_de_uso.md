# 3. Diagrama de Casos de Uso UML

## 3.1 Diagrama

![Diagrama de casos de uso](diagramas/casos_de_uso.png)

> Fuente editable: [`diagramas/casos_de_uso.puml`](diagramas/casos_de_uso.puml). Se puede abrir con la extensión *PlantUML* de VS Code o en https://www.plantuml.com.

**Lectura del diagrama:**
- El **Administrador de inventario** es una especialización del **Encargado de bodega** (generalización entre actores): puede hacer todo lo que hace el encargado y además administrar el maestro de datos.
- `<<include>>`: comportamiento que **siempre** se ejecuta como parte del caso base. Por ejemplo, todo registro o modificación valida datos y registra auditoría.
- `<<extend>>`: comportamiento **opcional o condicional**. La alerta de stock bajo el mínimo solo aparece al consultar productos cuando `Stock <= StockMinimo`.
- La **Base de datos MySQL** es un actor secundario: aplica la validación de stock y guarda la auditoría desde los SPs.

## 3.2 Trazabilidad casos de uso ↔ requerimientos ↔ implementación

| Caso de uso | Operación CRUD | RF | Procedimiento almacenado | Ventana (UI) |
|---|---|---|---|---|
| CU-01 Registrar producto | Create | RF-01 | `sp_producto_insertar` | VentanaProductos |
| CU-02 Consultar / buscar productos | Read | RF-02, RF-15 | `sp_producto_listar`, `sp_producto_obtener` | VentanaProductos |
| CU-03 Modificar producto | Update | RF-03 | `sp_producto_actualizar` | VentanaProductos |
| CU-04 Desactivar producto | Delete (lógico) | RF-04 | `sp_producto_eliminar` | VentanaProductos |
| CU-05 Registrar categoría | Create | RF-05 | `sp_categoria_insertar` | VentanaCategorias |
| CU-06 Listar categorías | Read | RF-06 | `sp_categoria_listar` | VentanaCategorias |
| CU-07 Modificar categoría | Update | RF-07 | `sp_categoria_actualizar` | VentanaCategorias |
| CU-08 Desactivar categoría | Delete (lógico) | RF-08 | `sp_categoria_eliminar` | VentanaCategorias |
| CU-09 Registrar proveedor | Create | RF-09 | `sp_proveedor_insertar` | VentanaProveedores |
| CU-10 Listar proveedores | Read | RF-10 | `sp_proveedor_listar` | VentanaProveedores |
| CU-11 Modificar proveedor | Update | RF-11 | `sp_proveedor_actualizar` | VentanaProveedores |
| CU-12 Desactivar proveedor | Delete (lógico) | RF-12 | `sp_proveedor_eliminar` | VentanaProveedores |
| CU-13 Registrar movimiento | Create | RF-13 | `sp_movimiento_registrar` | VentanaMovimientos |
| CU-14 Consultar kardex | Read | RF-14 | `sp_movimiento_listar` | VentanaMovimientos |

## 3.3 Especificación de casos de uso principales

### CU-01 Registrar producto

| Campo | Descripción |
|---|---|
| **Actor principal** | Administrador de inventario |
| **Precondición** | La aplicación está conectada a la BD y existe al menos una categoría y un proveedor activos. |
| **Postcondición** | El producto queda registrado con stock 0, estado activo y auditoría de creación. |
| **Flujo básico** | 1. El actor abre *Productos* y presiona **Nuevo**. 2. Ingresa SKU, nombre, descripción, categoría, proveedor, precios, stock mínimo, vencimiento y si requiere receta. 3. Presiona **Guardar**. 4. La UI valida el formato de los números. 5. La BLL valida las reglas de negocio (*include* Validar datos). 6. La DAL ejecuta `sp_producto_insertar`. 7. La BD guarda el registro y la auditoría (*include* Registrar auditoría). 8. El sistema muestra "Registro creado correctamente" y selecciona el producto en la grilla. |
| **Flujos alternativos** | 4a. Un precio no es numérico: se muestra "Precio de compra debe ser un número válido". 5a. Faltan datos, el SKU tiene formato inválido, el precio de venta es menor que el de compra o el vencimiento no es futuro: se muestran **todos** los errores juntos y no se guarda. 6a. El SKU ya existe (error 1062): "Ya existe un registro con ese valor". 6b. No hay conexión: "No se pudo conectar con el servidor MySQL" y el detalle queda en la bitácora. |
| **Seguridad** | A.8.26 / A.8.28: validación en tres capas y SP parametrizado. A.8.15: `UsuarioCreacion`. |

### CU-03 Modificar producto

| Campo | Descripción |
|---|---|
| **Actor principal** | Administrador de inventario |
| **Precondición** | El producto existe y está seleccionado en la grilla. |
| **Postcondición** | Los datos quedan actualizados, `Version` aumenta en 1 y se registra la auditoría de modificación. |
| **Flujo básico** | 1. El actor selecciona un producto y el formulario se carga con sus datos (*Editando: SKU - Nombre*). 2. Modifica los campos permitidos (el stock es de solo lectura). 3. Presiona **Guardar**. 4. Se valida igual que en CU-01. 5. Se ejecuta `sp_producto_actualizar` con el `Version` leído. 6. Se informa el éxito y se recarga la grilla. |
| **Flujos alternativos** | 5a. Otro usuario modificó el producto entre la lectura y el guardado: el SP devuelve "El producto no existe o fue modificado por otro usuario". El actor debe refrescar y repetir. |
| **Seguridad** | Integridad mediante concurrencia optimista. A.8.15: `UsuarioModificacion` y `FechaModificacion`. |

### CU-04 Desactivar producto

| Campo | Descripción |
|---|---|
| **Actor principal** | Administrador de inventario |
| **Precondición** | El producto está seleccionado. |
| **Postcondición** | El producto queda con `Activo = 0` y su kardex se conserva. |
| **Flujo básico** | 1. El actor presiona **Desactivar**. 2. El sistema pide confirmación (*include* Confirmar desactivación). 3. El actor confirma. 4. La BLL verifica que el stock sea 0. 5. Se ejecuta `sp_producto_eliminar`. 6. El producto deja de verse en la lista de activos. |
| **Flujos alternativos** | 3a. El actor cancela y no se hace nada. 4a. El stock es mayor que 0: "No se puede eliminar … porque tiene N unidades en stock. Registre primero una salida o un ajuste a 0". |
| **Seguridad** | A.5.33: sin borrado físico; la FK `ON DELETE RESTRICT` protege el historial. |

### CU-13 Registrar movimiento de stock

| Campo | Descripción |
|---|---|
| **Actor principal** | Encargado de bodega |
| **Precondición** | El producto está activo. |
| **Postcondición** | Existe un nuevo registro en `MovimientoStock` y `Producto.Stock` quedó actualizado en la misma transacción. |
| **Flujo básico** | 1. El actor abre *Movimientos de stock*. 2. Selecciona el producto y ve su stock actual y su kardex. 3. Elige el tipo (ENTRADA, SALIDA o AJUSTE), la cantidad, el motivo y el documento. 4. Presiona **Registrar movimiento**. 5. La UI valida que la cantidad sea un entero. 6. La BLL valida que la cantidad sea mayor que 0 y que SALIDA y AJUSTE tengan motivo. 7. `sp_movimiento_registrar` bloquea la fila del producto, calcula el nuevo stock (*include* Validar stock suficiente), inserta el movimiento, actualiza el stock y confirma (COMMIT). 8. Se muestra "stock anterior X, stock nuevo Y" y se actualiza el kardex. |
| **Flujos alternativos** | 7a. Una SALIDA supera el stock: ROLLBACK y mensaje "Stock insuficiente para la salida". 7b. El producto fue desactivado: "Producto inexistente o inactivo". |
| **Seguridad** | Integridad transaccional (todo o nada). A.8.15: el movimiento registra usuario, fecha y documento y no se puede editar ni borrar desde la aplicación. |

### CU-14 Consultar kardex

| Campo | Descripción |
|---|---|
| **Actor principal** | Encargado de bodega |
| **Flujo básico** | 1. El actor selecciona un producto en *Movimientos de stock*. 2. El sistema ejecuta `sp_movimiento_listar` y muestra los movimientos del más reciente al más antiguo. 3. Opcionalmente marca "Ver movimientos de todos los productos". |
| **Seguridad** | A.8.16: permite revisar quién hizo cada cambio de inventario. |

### CU-05 a CU-12 (Categorías y Proveedores)

Siguen el mismo patrón que CU-01 a CU-04 (Nuevo → Guardar → validar → SP → auditoría; Desactivar → confirmar → baja lógica). Diferencias específicas:

- **Categorías:** el nombre es obligatorio y único, con un máximo de 60 caracteres.
- **Proveedores:** el RUT es obligatorio, se valida con el algoritmo **módulo 11** y se guarda normalizado (`12345678-9`). El email se valida con una expresión regular y el teléfono solo admite números, espacios y `+`.
- Una categoría o un proveedor desactivado deja de aparecer en los combos de productos, pero los productos existentes conservan su referencia.
