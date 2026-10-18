# SIGBIM — Sistema de Gestión de Bodega de Insumos Médicos

Mantenedor CRUD de inventario (productos, categorías, proveedores y movimientos de stock) desarrollado en **C# / .NET 8** con **arquitectura de 4 capas**, **MySQL 8** con procedimientos almacenados y requerimientos de seguridad basados en **ISO/IEC 27001:2022**. Funciona en **macOS** (también en Windows y Linux).

## Estructura del repositorio

```
/docs       Informe técnico: problema, requerimientos ISO 27001, UML, modelo relacional, arquitectura,
           plan de pruebas con evidencias e informe integrador en PDF (docs/pdf)
/database   Scripts SQL: DDL, vista, procedimientos almacenados, datos de prueba y usuario de la app
/src        Solución C#: Sigbim.Entidades, Sigbim.DAL, Sigbim.BLL, Sigbim.UI (Avalonia)
```

El informe completo empieza en [`docs/README.md`](docs/README.md).

## Requisitos (Mac)

| Herramienta | Instalación |
|---|---|
| .NET SDK 8 (o superior) | https://dotnet.microsoft.com/download, o `brew install --cask dotnet-sdk` |
| MySQL Server 8 | https://dev.mysql.com/downloads/mysql/, o `brew install mysql && brew services start mysql` |
| MySQL Workbench | Para ejecutar los scripts y ver el diagrama ER |
| VS Code + extensión **C# Dev Kit** | Editor del proyecto |
| (Opcional) Extensión **PlantUML** | Para editar los diagramas `.puml` de `/docs/diagramas` |

## Instalación y ejecución

### 1. Base de datos

En MySQL Workbench, conectado a tu instancia local como `root`:

1. **File → Open SQL Script…** → `database/01_sigbim_inventario.sql` → ejecutar todo (⚡).
   Crea la BD `sigbim_mac`, las 4 tablas, la vista `vw_productodetalle`, los procedimientos almacenados y los datos de prueba.
2. Abrir `database/02_seguridad_usuario_app.sql`, **cambiar la contraseña** y ejecutarlo.
   Crea el usuario `sigbim_app`, que solo puede ejecutar procedimientos almacenados.
   El script elimina antes cualquier cuenta `sigbim_app` anterior (en `%` y en `localhost`): si quedara una
   antigua en `localhost`, MySQL la preferiría y la aplicación recibiría *Access denied* (ver docs/07).

Desde la terminal también se puede hacer así:

```bash
cd database
mysql -u root -p < 01_sigbim_inventario.sql
mysql -u root -p < 02_seguridad_usuario_app.sql
```

### 2. Configuración de la conexión

```bash
cd src/Sigbim.UI
cp appsettings.example.json appsettings.json
# Edita appsettings.json y pon la contraseña que definiste para sigbim_app
```

> `appsettings.json` está en `.gitignore`: tu contraseña **nunca** se sube a GitHub.
> Alternativa: definir la variable de entorno `SIGBIM_CONEXION` con la cadena de conexión.

### 3. Ejecutar

```bash
cd src
dotnet restore
dotnet run --project Sigbim.UI
```

La primera vez, `dotnet restore` descarga los paquetes **Avalonia** (interfaz) y **MySqlConnector** (driver).

## Uso de la aplicación

| Menú | Qué permite |
|---|---|
| **Productos** | Consulta de todos los productos registrados (solo lectura, con buscador). |
| **Movimientos de stock** | Registrar entradas, salidas y ajustes, y ver el kardex. |
| **Artículos y categorías** | Crear, modificar y desactivar artículos; mantenedor de categorías. |
| **Proveedores** | Mantenedor de proveedores (RUT validado con módulo 11). |
| **Bitácora y reporte PDF** | Ver todos los eventos (inicios de sesión, creaciones, modificaciones, desactivaciones, movimientos) y descargarlos en PDF. |

Cada ventana se abre sobre el menú principal: para volver, se cierra con el botón rojo. Cerrar la ventana principal cierra la aplicación.

## ¿Por qué Avalonia y no Windows Forms?

Windows Forms solo funciona en Windows. Para poder ejecutar el mantenedor en macOS, la capa de presentación usa **Avalonia UI**, que tiene el mismo modelo de ventanas, controles y eventos. Las capas **BLL, DAL y Entidades son las mismas** que se usarían con Windows Forms. La justificación completa está en [`docs/06_arquitectura.md`](docs/06_arquitectura.md#62-decisión-de-arquitectura-avalonia-ui-en-lugar-de-windows-forms).

## Seguridad aplicada (resumen ISO 27001)

- Usuario de BD con **mínimo privilegio** (solo `EXECUTE`): A.8.2 / A.8.3
- **Solo procedimientos almacenados** con parámetros, sin SQL concatenado: A.8.28
- Credenciales **fuera del código** y del repositorio: A.5.17
- **Auditoría** (usuario y fecha) en cada registro y **bitácora** en `logs/`: A.8.15
- **Baja lógica** (no se borra información): A.5.33
- Mensajes de error **sin detalles técnicos** para el usuario: A.8.12

## Historial de versiones

El historial de commits refleja el desarrollo por etapas: documentación (contexto, requerimientos, UML, modelo relacional, arquitectura), base de datos, las cuatro capas en C# y, después, los ajustes de la versión final (corrección del usuario de BD, menú reorganizado, bitácora en PDF, pruebas y documentación actualizada). Se revisa con:

```bash
git log --oneline
```
