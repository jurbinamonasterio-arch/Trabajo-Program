# SIGBIM — Sistema de Gestión de Bodega de Insumos Médicos

Mantenedor CRUD de inventario (productos, categorías, proveedores y movimientos de stock) desarrollado en **C# / .NET 8** con **arquitectura de 4 capas**, **MySQL 8** con procedimientos almacenados y requerimientos de seguridad basados en **ISO/IEC 27001:2022**. Funciona en **macOS** (también en Windows y Linux).

## Estructura del repositorio

```
/docs       Informe técnico: problema, requerimientos ISO 27001, UML, modelo relacional, arquitectura
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

## ¿Por qué Avalonia y no Windows Forms?

Windows Forms solo funciona en Windows. Para poder ejecutar el mantenedor en macOS, la capa de presentación usa **Avalonia UI**, que tiene el mismo modelo de ventanas, controles y eventos. Las capas **BLL, DAL y Entidades son las mismas** que se usarían con Windows Forms. La justificación completa está en [`docs/06_arquitectura.md`](docs/06_arquitectura.md#62-decisión-de-arquitectura-avalonia-ui-en-lugar-de-windows-forms).

## Seguridad aplicada (resumen ISO 27001)

- Usuario de BD con **mínimo privilegio** (solo `EXECUTE`): A.8.2 / A.8.3
- **Solo procedimientos almacenados** con parámetros, sin SQL concatenado: A.8.28
- Credenciales **fuera del código** y del repositorio: A.5.17
- **Auditoría** (usuario y fecha) en cada registro y **bitácora** en `logs/`: A.8.15
- **Baja lógica** (no se borra información): A.5.33
- Mensajes de error **sin detalles técnicos** para el usuario: A.8.12

## Plan de commits sugerido

Sube el proyecto **por etapas**, a medida que avances, para que el historial refleje un desarrollo incremental (criterio 1 de la rúbrica):

| # | Mensaje de commit | Contenido |
|---|---|---|
| 1 | `docs: estructura inicial del repositorio y README` | README, .gitignore, carpetas vacías |
| 2 | `docs: contexto y definición de la problemática` | `docs/01_contexto_y_problema.md` |
| 3 | `docs: requerimientos RF/RNF con tríada y controles ISO 27001` | `docs/02_requerimientos_iso27001.md` |
| 4 | `docs: diagrama de casos de uso UML del mantenedor CRUD` | `docs/03_*`, `diagramas/casos_de_uso.*` |
| 5 | `docs: diagrama de clases UML (dominio y capas)` | `docs/04_*`, `diagramas/clases_*` |
| 6 | `docs: modelo relacional y proceso de normalización 1FN-3FN` | `docs/05_*`, `diagramas/modelo_relacional.*` |
| 7 | `db: DDL de tablas con PK, FK, UNIQUE y CHECK` | Sección 1 del script SQL |
| 8 | `db: vista consolidada y procedimientos almacenados CRUD` | Secciones 2–4 del script SQL |
| 9 | `db: usuario de aplicación con mínimo privilegio` | `database/02_seguridad_usuario_app.sql` |
| 10 | `docs: arquitectura en 4 capas y flujo de datos` | `docs/06_arquitectura.md` |
| 11 | `feat: capa de entidades y excepciones` | `src/Sigbim.Entidades` |
| 12 | `feat: capa DAL con invocación de SPs y traducción de errores` | `src/Sigbim.DAL` |
| 13 | `feat: capa BLL con validaciones y bitácora` | `src/Sigbim.BLL` |
| 14 | `feat: UI principal y mantenedor de categorías` | `App`, `Ui`, `VentanaPrincipal`, `VentanaMantenedor`, `VentanaCategorias` |
| 15 | `feat: mantenedores de proveedores y productos` | `VentanaProveedores`, `VentanaProductos` |
| 16 | `feat: registro de movimientos de stock y kardex` | `VentanaMovimientos` |
| 17 | `fix/test: pruebas funcionales y ajustes finales` | Correcciones tras probar en tu Mac |

```bash
git init
git add README.md .gitignore
git commit -m "docs: estructura inicial del repositorio y README"
git branch -M main
git remote add origin https://github.com/<tu-usuario>/sigbim-inventario.git
git push -u origin main
```
