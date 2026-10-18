# 2. Especificación de requerimientos con ISO/IEC 27001

## 2.1 Metodología

Cada **requerimiento funcional (RF)** se especifica con la **tríada de requerimientos**:

1. **Rol / Usuario:** quién lo necesita.
2. **Necesidad / Acción:** qué debe poder hacer.
3. **Criterio de aceptación / Seguridad:** cómo se verifica que está cumplido y qué control de seguridad lo protege.

Los controles de seguridad se toman del **Anexo A de ISO/IEC 27001:2022**. Se usan principalmente los controles tecnológicos (8.x) y algunos organizacionales (5.x) que aplican al software.

### Controles ISO/IEC 27001:2022 aplicados

| Control | Nombre | Cómo se aplica en SIGBIM |
|---|---|---|
| A.5.15 | Control de acceso | Cada capa accede solo a la siguiente; la UI no conoce la base de datos. |
| A.5.17 | Información de autenticación | La contraseña de BD no está en el código y `appsettings.json` se excluye de Git. |
| A.5.33 | Protección de registros | Baja lógica: no se borran productos, categorías, proveedores ni movimientos. |
| A.8.2 | Derechos de acceso privilegiado | La app no usa `root`; usa el usuario `sigbim_app`. |
| A.8.3 | Restricción de acceso a la información | `sigbim_app` solo tiene permiso `EXECUTE` sobre los SPs, sin `SELECT` ni `UPDATE` directos. |
| A.8.4 | Acceso al código fuente | Repositorio GitHub privado con historial de cambios. |
| A.8.9 | Gestión de la configuración | Configuración externa (`appsettings.json` o variable de entorno) con plantilla `.example`. |
| A.8.12 | Prevención de fuga de datos | Los errores técnicos no se muestran al usuario; solo van a la bitácora. |
| A.8.13 | Respaldo de la información | Procedimiento de respaldo con `mysqldump` (sección 2.4). |
| A.8.15 | Registro (logging) | Columnas de auditoría, tabla `MovimientoStock`, bitácora de la aplicación en `/logs` y su reporte en PDF (RF-17 y RF-18). |
| A.8.25 | Ciclo de vida de desarrollo seguro | Requerimientos de seguridad definidos antes de codificar (este documento). |
| A.8.26 | Requisitos de seguridad de las aplicaciones | Validación de entradas en UI, BLL y BD. |
| A.8.28 | Codificación segura | Solo SPs con parámetros: nunca se concatena SQL (previene inyección SQL). |
| A.8.29 | Pruebas de seguridad en desarrollo y aceptación | Casos de prueba negativos (stock negativo, duplicados, RUT inválido). |
| A.8.32 | Gestión de cambios | Commits descriptivos e incrementales en GitHub. |

---

## 2.2 Requerimientos funcionales (tríada + ISO 27001)

### Módulo Productos

> **Organización del menú (versión final):** el menú **Productos** es solo de **consulta** (RF-02). La creación, modificación y desactivación de artículos (RF-01, RF-03, RF-04) se realiza desde **Artículos y categorías → Gestión de artículos**, el mismo apartado que contiene el mantenedor de categorías (RF-05 a RF-08). Así las operaciones que modifican el catálogo quedan concentradas en un único punto.

| ID | Rol / Usuario | Necesidad / Acción | Criterio de aceptación | Criterio de seguridad (ISO 27001) |
|---|---|---|---|---|
| **RF-01** | Administrador de inventario | Registrar un producto nuevo con SKU, nombre, descripción, categoría, proveedor, precios, stock mínimo, vencimiento y si requiere receta. | El producto queda guardado con stock 0, estado activo y SKU en mayúsculas. Se rechaza si el SKU ya existe, si falta un campo obligatorio, si el precio de venta es menor que el de compra o si la fecha de vencimiento no es futura. | A.8.26 / A.8.28: validación en UI, BLL y BD (UNIQUE y CHECK) e inserción solo vía `sp_producto_insertar`. A.8.15: guarda `UsuarioCreacion` y `FechaCreacion`. |
| **RF-02** | Encargado de bodega | Consultar y buscar productos por SKU o nombre, viendo categoría, proveedor, stock y estado. | La grilla muestra los productos activos (o todos si se marca "Mostrar inactivos"), con estado de stock OK, BAJO MÍNIMO o SIN STOCK. La ventana es de **solo lectura**: no muestra formulario ni botones de edición. | A.8.3: la lectura se hace mediante `sp_producto_listar` sobre la vista; la app no tiene `SELECT` directo sobre las tablas. |
| **RF-03** | Administrador de inventario | Modificar los datos de un producto existente. | Se guardan los cambios y aumenta `Version`. Si otro usuario modificó el producto antes, se muestra "fue modificado por otro usuario" y no se sobrescribe. El stock **no** se edita desde este formulario. | A.8.15: guarda `UsuarioModificacion` y `FechaModificacion`. Integridad: concurrencia optimista (`Version`). |
| **RF-04** | Administrador de inventario | Desactivar (dar de baja) un producto. | Pide confirmación y el producto queda con `Activo = 0`. No se permite si el producto tiene stock mayor que 0. | A.5.33: baja lógica, sin borrado físico, que conserva el kardex. |

### Módulo Categorías

| ID | Rol / Usuario | Necesidad / Acción | Criterio de aceptación | Criterio de seguridad (ISO 27001) |
|---|---|---|---|---|
| **RF-05** | Administrador de inventario | Crear una categoría con nombre y descripción. | Se guarda activa. Se rechaza si el nombre está vacío, supera 60 caracteres o ya existe. | A.8.26: validación de largo y unicidad (UNIQUE). A.8.15: auditoría de creación. |
| **RF-06** | Administrador de inventario | Listar las categorías (activas o todas). | La grilla muestra nombre, descripción, estado y auditoría. | A.8.3: lectura solo vía `sp_categoria_listar`. |
| **RF-07** | Administrador de inventario | Modificar o reactivar una categoría. | Los cambios se reflejan en la grilla y en los productos asociados. | A.8.15: auditoría de modificación. |
| **RF-08** | Administrador de inventario | Desactivar una categoría. | Pide confirmación. La categoría deja de aparecer en el combo de productos nuevos. | A.5.33: baja lógica. La FK con `ON DELETE RESTRICT` impide el borrado físico. |

### Módulo Proveedores

| ID | Rol / Usuario | Necesidad / Acción | Criterio de aceptación | Criterio de seguridad (ISO 27001) |
|---|---|---|---|---|
| **RF-09** | Administrador de inventario | Registrar un proveedor con RUT, razón social, contacto, email, teléfono, dirección y ciudad. | Se guarda si el RUT es válido (dígito verificador módulo 11) y no está repetido, y si el email tiene formato válido. El RUT se guarda normalizado (`12345678-9`). | A.8.26: validación de formato en BLL y unicidad en BD. A.8.15: auditoría de creación. |
| **RF-10** | Administrador de inventario | Listar los proveedores (activos o todos). | La grilla muestra RUT, razón social, contacto y estado. | A.8.3: lectura solo vía `sp_proveedor_listar`. A.8.12: los datos de contacto solo se muestran dentro de la aplicación. |
| **RF-11** | Administrador de inventario | Modificar o reactivar un proveedor. | Los cambios quedan guardados con auditoría. | A.8.15: auditoría de modificación. |
| **RF-12** | Administrador de inventario | Desactivar un proveedor. | Pide confirmación y el proveedor deja de aparecer para productos nuevos. | A.5.33: baja lógica. |

### Módulo Movimientos de stock

| ID | Rol / Usuario | Necesidad / Acción | Criterio de aceptación | Criterio de seguridad (ISO 27001) |
|---|---|---|---|---|
| **RF-13** | Encargado de bodega | Registrar una ENTRADA, SALIDA o AJUSTE de stock de un producto, indicando cantidad, motivo y documento de referencia. | El stock se actualiza (ENTRADA suma, SALIDA resta, AJUSTE fija el valor) y se guarda el movimiento con stock anterior, stock nuevo, fecha y usuario. Una SALIDA mayor que el stock se rechaza con "Stock insuficiente". SALIDA y AJUSTE exigen motivo. | Integridad: transacción con `SELECT … FOR UPDATE` dentro de `sp_movimiento_registrar` (todo o nada). A.8.15: el movimiento es un registro de auditoría inmutable. |
| **RF-14** | Encargado de bodega | Consultar el kardex (historial) de un producto o de todos. | Se listan los movimientos del más reciente al más antiguo, con tipo, cantidad, stock anterior y nuevo, motivo, documento y usuario. | A.8.15 / A.8.16: permite revisar y monitorear quién modificó el inventario. |

### Módulo General

| ID | Rol / Usuario | Necesidad / Acción | Criterio de aceptación | Criterio de seguridad (ISO 27001) |
|---|---|---|---|---|
| **RF-15** | Encargado / Administrador | Ver en una sola consulta todos los datos de un producto (categoría, proveedor, margen, estado y valor del inventario). | La vista `vw_productodetalle` entrega los datos consolidados y calculados sin duplicarlos en tablas. | Integridad: los datos calculados no se almacenan, por lo que no pueden quedar desactualizados. |
| **RF-16** | Encargado / Administrador | Saber al iniciar si la aplicación está conectada a la base de datos. | La ventana principal muestra "Conectado". Si no hay conexión, deshabilita el menú y muestra un mensaje claro. | A.8.12: el mensaje no expone el servidor, el usuario ni la contraseña. A.8.15: el intento queda en la bitácora. |

### Módulo Bitácora y reportes

| ID | Rol / Usuario | Necesidad / Acción | Criterio de aceptación | Criterio de seguridad (ISO 27001) |
|---|---|---|---|---|
| **RF-17** | Administrador / Auditor | Consultar en pantalla la bitácora: inicios de sesión, creaciones, modificaciones, desactivaciones y movimientos de stock, con fecha, usuario, operación y detalle. | La ventana **Bitácora y reporte PDF** lista todos los eventos registrados, del más reciente al más antiguo, con el total de eventos. | A.8.15 / A.8.16: revisión y monitoreo de la actividad. A.8.12: la bitácora nunca contiene contraseñas ni la cadena de conexión. |
| **RF-18** | Administrador / Auditor | Descargar la bitácora completa en un archivo PDF. | El botón **Descargar PDF** pide dónde guardar el archivo y genera un PDF con encabezado (quién lo generó, cuándo y el período cubierto), resumen por usuario y por operación, y el detalle cronológico de todos los eventos. El PDF se abre automáticamente. | A.8.15: cada exportación queda registrada en la bitácora (`Exportar bitácora PDF`, con el nombre del archivo y la cantidad de eventos), de modo que también es trazable quién extrajo la información. |

---

## 2.3 Requerimientos no funcionales

| ID | Categoría | Requerimiento | Control ISO 27001 | Verificación |
|---|---|---|---|---|
| **RNF-01** | Seguridad: acceso | La aplicación se conecta con un usuario de BD de mínimo privilegio que solo puede ejecutar SPs. | A.8.2, A.8.3 | Script `database/02_seguridad_usuario_app.sql`. Un `SELECT` directo con `sigbim_app` devuelve *SELECT command denied*. |
| **RNF-02** | Seguridad: credenciales | La cadena de conexión no se escribe en el código ni se sube a GitHub. | A.5.17, A.8.9 | `appsettings.json` está en `.gitignore` y solo se versiona `appsettings.example.json`. |
| **RNF-03** | Seguridad: codificación | Toda la manipulación de datos se hace con SPs y parámetros, sin SQL concatenado. | A.8.28 | Revisión del código de la DAL (`Db.cs`: solo `CALL sp_…` con parámetros `@p_…`). |
| **RNF-04** | Seguridad: trazabilidad | Cada operación de escritura registra usuario y fecha, y queda en la bitácora `logs/sigbim-AAAAMMDD.log`. | A.8.15 | Revisar las columnas de auditoría y el archivo de log después de una operación. |
| **RNF-05** | Seguridad: errores | El usuario ve mensajes comprensibles. El detalle técnico solo se guarda en la bitácora. | A.8.12 | Forzar un error (MySQL apagado) y verificar que el mensaje no muestre datos internos. |
| **RNF-06** | Integridad | El stock nunca puede quedar negativo ni cambiar sin un movimiento asociado. | A.8.26 | CHECK `Stock >= 0`, SP transaccional y stock de solo lectura en la UI. |
| **RNF-07** | Integridad | La BD debe estar en 3FN, con PK, FK, UNIQUE y CHECK. | A.8.26 | Documento 05 y script DDL. |
| **RNF-08** | Disponibilidad | Se debe poder respaldar y restaurar la BD. | A.8.13 | Procedimiento de la sección 2.4. |
| **RNF-09** | Portabilidad | La aplicación debe ejecutarse en macOS (Apple Silicon e Intel) y también en Windows. | — | .NET 8 + Avalonia UI + MySqlConnector, todos multiplataforma. |
| **RNF-10** | Mantenibilidad | Arquitectura en 4 capas: la UI no referencia la DAL y la DAL no referencia la UI. | A.8.25 | Referencias entre proyectos en los `.csproj` (documento 06). |
| **RNF-11** | Usabilidad | Los formularios marcan los campos obligatorios con `*`, muestran todos los errores de validación juntos y piden confirmación antes de desactivar. | — | Prueba manual de los mantenedores. |
| **RNF-12** | Rendimiento | Las búsquedas y listados responden en menos de 2 segundos con hasta 10.000 productos. | — | Índices en FK e índice `(IdProducto, Fecha)` en movimientos. |
| **RNF-13** | Gestión de cambios | Todo cambio queda versionado en GitHub con commits descriptivos e incrementales. | A.8.32, A.8.4 | Historial de commits del repositorio. |

## 2.4 Procedimiento de respaldo (RNF-08)

```bash
# Respaldo (incluye procedimientos almacenados y vistas)
mysqldump -u root -p --routines --triggers sigbim_mac > respaldo_sigbim_$(date +%Y%m%d).sql

# Restauración
mysql -u root -p < respaldo_sigbim_AAAAMMDD.sql
```

## 2.5 Validaciones por capa (matriz)

| Regla | UI | BLL | Base de datos |
|---|---|---|---|
| Campos obligatorios | Marcados con `*` | `Validador.Requerido` | `NOT NULL` |
| Largo máximo de textos | `MaxLength` del TextBox | `Validador.LargoMaximo` | `VARCHAR(n)` |
| Números válidos (precios, cantidades) | `Ui.LeerDecimal` / `Ui.LeerEntero` | Rangos (`>= 0`, `> 0`) | `CHECK` |
| SKU / RUT / Email con formato | — | Regex y algoritmo módulo 11 | `UNIQUE` |
| Precio venta ≥ precio compra | — | `ProductoBLL.Validar` | `SIGNAL` en el SP |
| Stock suficiente para una salida | — | — | `SIGNAL` en `sp_movimiento_registrar` |
| No desactivar un producto con stock | — | `ProductoBLL.Eliminar` | — |
| Concurrencia (dos usuarios editando) | — | — | `WHERE Version = p_Version` |
