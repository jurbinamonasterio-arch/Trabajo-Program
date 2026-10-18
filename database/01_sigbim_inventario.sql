-- =====================================================================
-- SIGBIM - Módulo de Inventario (MySQL 8 / MySQL Workbench en Mac)
-- Tablas: Categoria, Proveedor, Producto, MovimientoStock
-- Vista:  vw_productodetalle (consolida todos los datos)
-- SPs:    CRUD de Categoria, Proveedor, Producto + registro de movimientos
-- =====================================================================

DROP DATABASE IF EXISTS sigbim_mac;
CREATE DATABASE sigbim_mac CHARACTER SET utf8mb4 COLLATE utf8mb4_spanish_ci;
USE sigbim_mac;

-- ---------------------------------------------------------------------
-- 1. TABLAS
-- ---------------------------------------------------------------------

CREATE TABLE Categoria (
    IdCategoria           INT          NOT NULL AUTO_INCREMENT,
    Nombre                VARCHAR(60)  NOT NULL,
    Descripcion           VARCHAR(200) NULL,
    Activo                TINYINT(1)   NOT NULL DEFAULT 1,
    Version               INT          NOT NULL DEFAULT 1,
    FechaCreacion         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioCreacion       VARCHAR(50)  NOT NULL,
    FechaModificacion     DATETIME     NULL,
    UsuarioModificacion   VARCHAR(50)  NULL,
    CONSTRAINT PK_Categoria PRIMARY KEY (IdCategoria),
    CONSTRAINT UQ_Categoria_Nombre UNIQUE (Nombre)
) ENGINE = InnoDB;

CREATE TABLE Proveedor (
    IdProveedor           INT          NOT NULL AUTO_INCREMENT,
    Rut                   VARCHAR(12)  NOT NULL,
    RazonSocial           VARCHAR(120) NOT NULL,
    NombreContacto        VARCHAR(120) NULL,
    Email                 VARCHAR(120) NULL,
    Telefono              VARCHAR(20)  NULL,
    Direccion             VARCHAR(150) NULL,
    Ciudad                VARCHAR(60)  NULL,
    Activo                TINYINT(1)   NOT NULL DEFAULT 1,
    Version               INT          NOT NULL DEFAULT 1,
    FechaCreacion         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioCreacion       VARCHAR(50)  NOT NULL,
    FechaModificacion     DATETIME     NULL,
    UsuarioModificacion   VARCHAR(50)  NULL,
    CONSTRAINT PK_Proveedor PRIMARY KEY (IdProveedor),
    CONSTRAINT UQ_Proveedor_Rut UNIQUE (Rut)
) ENGINE = InnoDB;

CREATE TABLE Producto (
    IdProducto            INT           NOT NULL AUTO_INCREMENT,
    Sku                   VARCHAR(20)   NOT NULL,
    Nombre                VARCHAR(120)  NOT NULL,
    Descripcion           VARCHAR(250)  NULL,
    IdCategoria           INT           NOT NULL,
    IdProveedor           INT           NOT NULL,
    PrecioCompra          DECIMAL(12,2) NOT NULL DEFAULT 0,
    PrecioVenta           DECIMAL(12,2) NOT NULL DEFAULT 0,
    Stock                 INT           NOT NULL DEFAULT 0,
    StockMinimo           INT           NOT NULL DEFAULT 0,
    FechaVencimiento      DATE          NULL,
    RequiereReceta        TINYINT(1)    NOT NULL DEFAULT 0,
    Activo                TINYINT(1)    NOT NULL DEFAULT 1,
    Version               INT           NOT NULL DEFAULT 1,
    FechaCreacion         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioCreacion       VARCHAR(50)   NOT NULL,
    FechaModificacion     DATETIME      NULL,
    UsuarioModificacion   VARCHAR(50)   NULL,
    CONSTRAINT PK_Producto PRIMARY KEY (IdProducto),
    CONSTRAINT UQ_Producto_Sku UNIQUE (Sku),
    CONSTRAINT FK_Producto_Categoria FOREIGN KEY (IdCategoria)
        REFERENCES Categoria (IdCategoria) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT FK_Producto_Proveedor FOREIGN KEY (IdProveedor)
        REFERENCES Proveedor (IdProveedor) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT CK_Producto_Precios CHECK (PrecioCompra >= 0 AND PrecioVenta >= 0),
    CONSTRAINT CK_Producto_Stock   CHECK (Stock >= 0 AND StockMinimo >= 0)
) ENGINE = InnoDB;

CREATE TABLE MovimientoStock (
    IdMovimiento          INT          NOT NULL AUTO_INCREMENT,
    IdProducto            INT          NOT NULL,
    Tipo                  VARCHAR(10)  NOT NULL,          -- ENTRADA / SALIDA / AJUSTE
    Cantidad              INT          NOT NULL,
    StockAnterior         INT          NOT NULL,
    StockNuevo            INT          NOT NULL,
    Motivo                VARCHAR(200) NULL,
    DocumentoReferencia   VARCHAR(40)  NULL,
    Fecha                 DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioResponsable    VARCHAR(50)  NOT NULL,
    CONSTRAINT PK_MovimientoStock PRIMARY KEY (IdMovimiento),
    CONSTRAINT FK_Movimiento_Producto FOREIGN KEY (IdProducto)
        REFERENCES Producto (IdProducto) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT CK_Movimiento_Tipo     CHECK (Tipo IN ('ENTRADA','SALIDA','AJUSTE')),
    CONSTRAINT CK_Movimiento_Cantidad CHECK (Cantidad > 0)
) ENGINE = InnoDB;

CREATE INDEX IX_Producto_Categoria  ON Producto (IdCategoria);
CREATE INDEX IX_Producto_Proveedor  ON Producto (IdProveedor);
CREATE INDEX IX_Movimiento_Producto ON MovimientoStock (IdProducto, Fecha);

-- ---------------------------------------------------------------------
-- 2. VISTA CONSOLIDADA (todos los datos en un solo lugar)
-- ---------------------------------------------------------------------

CREATE OR REPLACE VIEW vw_productodetalle AS
SELECT
    p.IdProducto,
    p.Sku,
    p.Nombre                 AS Producto,
    p.Descripcion,
    c.IdCategoria,
    c.Nombre                 AS Categoria,
    pr.IdProveedor,
    pr.Rut                   AS RutProveedor,
    pr.RazonSocial           AS Proveedor,
    pr.NombreContacto        AS ContactoProveedor,
    pr.Telefono              AS TelefonoProveedor,
    p.PrecioCompra,
    p.PrecioVenta,
    (p.PrecioVenta - p.PrecioCompra)            AS Margen,
    p.Stock,
    p.StockMinimo,
    CASE WHEN p.Stock = 0              THEN 'SIN STOCK'
         WHEN p.Stock <= p.StockMinimo THEN 'BAJO MINIMO'
         ELSE 'OK' END                          AS EstadoStock,
    (p.Stock * p.PrecioCompra)                  AS ValorInventario,
    p.FechaVencimiento,
    p.RequiereReceta,
    p.Activo,
    p.Version,
    (SELECT COUNT(*)   FROM MovimientoStock m WHERE m.IdProducto = p.IdProducto) AS TotalMovimientos,
    (SELECT MAX(Fecha) FROM MovimientoStock m WHERE m.IdProducto = p.IdProducto) AS UltimoMovimiento,
    p.FechaCreacion,
    p.UsuarioCreacion,
    p.FechaModificacion,
    p.UsuarioModificacion
FROM Producto p
INNER JOIN Categoria c  ON c.IdCategoria  = p.IdCategoria
INNER JOIN Proveedor pr ON pr.IdProveedor = p.IdProveedor;

-- ---------------------------------------------------------------------
-- 3. PROCEDIMIENTOS ALMACENADOS
-- ---------------------------------------------------------------------
DELIMITER $$

-- ===== CATEGORIA =====
CREATE PROCEDURE sp_categoria_insertar(
    IN p_Nombre VARCHAR(60), IN p_Descripcion VARCHAR(200), IN p_Usuario VARCHAR(50))
BEGIN
    INSERT INTO Categoria (Nombre, Descripcion, UsuarioCreacion)
    VALUES (TRIM(p_Nombre), p_Descripcion, p_Usuario);
    SELECT LAST_INSERT_ID() AS IdCategoria;
END$$

CREATE PROCEDURE sp_categoria_actualizar(
    IN p_IdCategoria INT, IN p_Nombre VARCHAR(60), IN p_Descripcion VARCHAR(200),
    IN p_Activo TINYINT, IN p_Usuario VARCHAR(50))
BEGIN
    UPDATE Categoria
       SET Nombre = TRIM(p_Nombre), Descripcion = p_Descripcion, Activo = p_Activo,
           Version = Version + 1, FechaModificacion = NOW(), UsuarioModificacion = p_Usuario
     WHERE IdCategoria = p_IdCategoria;
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

CREATE PROCEDURE sp_categoria_eliminar(IN p_IdCategoria INT, IN p_Usuario VARCHAR(50))
BEGIN
    -- Baja lógica: se conserva el historial (trazabilidad ISO 27001)
    UPDATE Categoria
       SET Activo = 0, Version = Version + 1, FechaModificacion = NOW(), UsuarioModificacion = p_Usuario
     WHERE IdCategoria = p_IdCategoria;
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

CREATE PROCEDURE sp_categoria_listar(IN p_SoloActivos TINYINT)
BEGIN
    SELECT * FROM Categoria
     WHERE (p_SoloActivos = 0 OR Activo = 1)
     ORDER BY Nombre;
END$$

-- ===== PROVEEDOR =====
CREATE PROCEDURE sp_proveedor_insertar(
    IN p_Rut VARCHAR(12), IN p_RazonSocial VARCHAR(120), IN p_NombreContacto VARCHAR(120),
    IN p_Email VARCHAR(120), IN p_Telefono VARCHAR(20), IN p_Direccion VARCHAR(150),
    IN p_Ciudad VARCHAR(60), IN p_Usuario VARCHAR(50))
BEGIN
    INSERT INTO Proveedor (Rut, RazonSocial, NombreContacto, Email, Telefono, Direccion, Ciudad, UsuarioCreacion)
    VALUES (TRIM(p_Rut), TRIM(p_RazonSocial), p_NombreContacto, p_Email, p_Telefono, p_Direccion, p_Ciudad, p_Usuario);
    SELECT LAST_INSERT_ID() AS IdProveedor;
END$$

CREATE PROCEDURE sp_proveedor_actualizar(
    IN p_IdProveedor INT, IN p_Rut VARCHAR(12), IN p_RazonSocial VARCHAR(120),
    IN p_NombreContacto VARCHAR(120), IN p_Email VARCHAR(120), IN p_Telefono VARCHAR(20),
    IN p_Direccion VARCHAR(150), IN p_Ciudad VARCHAR(60), IN p_Activo TINYINT, IN p_Usuario VARCHAR(50))
BEGIN
    UPDATE Proveedor
       SET Rut = TRIM(p_Rut), RazonSocial = TRIM(p_RazonSocial), NombreContacto = p_NombreContacto,
           Email = p_Email, Telefono = p_Telefono, Direccion = p_Direccion, Ciudad = p_Ciudad,
           Activo = p_Activo, Version = Version + 1,
           FechaModificacion = NOW(), UsuarioModificacion = p_Usuario
     WHERE IdProveedor = p_IdProveedor;
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

CREATE PROCEDURE sp_proveedor_eliminar(IN p_IdProveedor INT, IN p_Usuario VARCHAR(50))
BEGIN
    UPDATE Proveedor
       SET Activo = 0, Version = Version + 1, FechaModificacion = NOW(), UsuarioModificacion = p_Usuario
     WHERE IdProveedor = p_IdProveedor;
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

CREATE PROCEDURE sp_proveedor_listar(IN p_SoloActivos TINYINT)
BEGIN
    SELECT * FROM Proveedor
     WHERE (p_SoloActivos = 0 OR Activo = 1)
     ORDER BY RazonSocial;
END$$

-- ===== PRODUCTO =====
CREATE PROCEDURE sp_producto_insertar(
    IN p_Sku VARCHAR(20), IN p_Nombre VARCHAR(120), IN p_Descripcion VARCHAR(250),
    IN p_IdCategoria INT, IN p_IdProveedor INT,
    IN p_PrecioCompra DECIMAL(12,2), IN p_PrecioVenta DECIMAL(12,2),
    IN p_StockMinimo INT, IN p_FechaVencimiento DATE, IN p_RequiereReceta TINYINT,
    IN p_Usuario VARCHAR(50))
BEGIN
    IF p_PrecioVenta < p_PrecioCompra THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El precio de venta no puede ser menor al precio de compra';
    END IF;

    -- El stock inicial parte en 0; se carga con sp_movimiento_registrar (ENTRADA)
    INSERT INTO Producto (Sku, Nombre, Descripcion, IdCategoria, IdProveedor, PrecioCompra, PrecioVenta,
                          Stock, StockMinimo, FechaVencimiento, RequiereReceta, UsuarioCreacion)
    VALUES (UPPER(TRIM(p_Sku)), TRIM(p_Nombre), p_Descripcion, p_IdCategoria, p_IdProveedor,
            p_PrecioCompra, p_PrecioVenta, 0, p_StockMinimo, p_FechaVencimiento, p_RequiereReceta, p_Usuario);
    SELECT LAST_INSERT_ID() AS IdProducto;
END$$

CREATE PROCEDURE sp_producto_actualizar(
    IN p_IdProducto INT, IN p_Sku VARCHAR(20), IN p_Nombre VARCHAR(120), IN p_Descripcion VARCHAR(250),
    IN p_IdCategoria INT, IN p_IdProveedor INT,
    IN p_PrecioCompra DECIMAL(12,2), IN p_PrecioVenta DECIMAL(12,2),
    IN p_StockMinimo INT, IN p_FechaVencimiento DATE, IN p_RequiereReceta TINYINT,
    IN p_Activo TINYINT, IN p_Version INT, IN p_Usuario VARCHAR(50))
BEGIN
    IF p_PrecioVenta < p_PrecioCompra THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El precio de venta no puede ser menor al precio de compra';
    END IF;

    -- El stock NO se edita aquí: solo cambia vía movimientos (integridad del inventario)
    UPDATE Producto
       SET Sku = UPPER(TRIM(p_Sku)), Nombre = TRIM(p_Nombre), Descripcion = p_Descripcion,
           IdCategoria = p_IdCategoria, IdProveedor = p_IdProveedor,
           PrecioCompra = p_PrecioCompra, PrecioVenta = p_PrecioVenta,
           StockMinimo = p_StockMinimo, FechaVencimiento = p_FechaVencimiento,
           RequiereReceta = p_RequiereReceta, Activo = p_Activo,
           Version = Version + 1, FechaModificacion = NOW(), UsuarioModificacion = p_Usuario
     WHERE IdProducto = p_IdProducto
       AND Version = p_Version;   -- control de concurrencia optimista

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El producto no existe o fue modificado por otro usuario';
    END IF;
    SELECT 1 AS FilasAfectadas;
END$$

CREATE PROCEDURE sp_producto_eliminar(IN p_IdProducto INT, IN p_Usuario VARCHAR(50))
BEGIN
    UPDATE Producto
       SET Activo = 0, Version = Version + 1, FechaModificacion = NOW(), UsuarioModificacion = p_Usuario
     WHERE IdProducto = p_IdProducto;
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

CREATE PROCEDURE sp_producto_obtener(IN p_IdProducto INT)
BEGIN
    SELECT * FROM vw_productodetalle WHERE IdProducto = p_IdProducto;
END$$

CREATE PROCEDURE sp_producto_listar(IN p_Buscar VARCHAR(120), IN p_SoloActivos TINYINT)
BEGIN
    SELECT * FROM vw_productodetalle
     WHERE (p_SoloActivos = 0 OR Activo = 1)
       AND (p_Buscar IS NULL OR p_Buscar = ''
            OR Sku LIKE CONCAT('%', p_Buscar, '%')
            OR Producto LIKE CONCAT('%', p_Buscar, '%'))
     ORDER BY Producto;
END$$

-- ===== MOVIMIENTO DE STOCK =====
CREATE PROCEDURE sp_movimiento_registrar(
    IN p_IdProducto INT, IN p_Tipo VARCHAR(10), IN p_Cantidad INT,
    IN p_Motivo VARCHAR(200), IN p_Documento VARCHAR(40), IN p_Usuario VARCHAR(50))
BEGIN
    DECLARE v_StockAnterior INT;
    DECLARE v_StockNuevo    INT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    IF p_Cantidad IS NULL OR p_Cantidad <= 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La cantidad debe ser mayor a 0';
    END IF;

    START TRANSACTION;

    -- Bloquea la fila del producto mientras se calcula el nuevo stock
    SELECT Stock INTO v_StockAnterior
      FROM Producto
     WHERE IdProducto = p_IdProducto AND Activo = 1
     FOR UPDATE;

    IF v_StockAnterior IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Producto inexistente o inactivo';
    END IF;

    SET v_StockNuevo = CASE UPPER(p_Tipo)
                           WHEN 'ENTRADA' THEN v_StockAnterior + p_Cantidad
                           WHEN 'SALIDA'  THEN v_StockAnterior - p_Cantidad
                           WHEN 'AJUSTE'  THEN p_Cantidad       -- AJUSTE fija el stock al conteo físico
                       END;

    IF v_StockNuevo IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tipo de movimiento inválido (ENTRADA, SALIDA, AJUSTE)';
    END IF;
    IF v_StockNuevo < 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Stock insuficiente para la salida';
    END IF;

    INSERT INTO MovimientoStock (IdProducto, Tipo, Cantidad, StockAnterior, StockNuevo,
                                 Motivo, DocumentoReferencia, UsuarioResponsable)
    VALUES (p_IdProducto, UPPER(p_Tipo), p_Cantidad, v_StockAnterior, v_StockNuevo,
            p_Motivo, p_Documento, p_Usuario);

    UPDATE Producto
       SET Stock = v_StockNuevo, Version = Version + 1,
           FechaModificacion = NOW(), UsuarioModificacion = p_Usuario
     WHERE IdProducto = p_IdProducto;

    COMMIT;
    SELECT LAST_INSERT_ID() AS IdMovimiento, v_StockNuevo AS StockNuevo;
END$$

CREATE PROCEDURE sp_movimiento_listar(IN p_IdProducto INT)
BEGIN
    SELECT m.*, p.Sku, p.Nombre AS Producto
      FROM MovimientoStock m
      INNER JOIN Producto p ON p.IdProducto = m.IdProducto
     WHERE (p_IdProducto IS NULL OR m.IdProducto = p_IdProducto)
     ORDER BY m.Fecha DESC;
END$$

DELIMITER ;

-- ---------------------------------------------------------------------
-- 4. DATOS DE PRUEBA
-- ---------------------------------------------------------------------
CALL sp_categoria_insertar('Insumos clínicos', 'Gasas, jeringas, guantes', 'admin');
CALL sp_categoria_insertar('Medicamentos', 'Fármacos de uso general', 'admin');

CALL sp_proveedor_insertar('76123456-0', 'Distribuidora Médica Sur SpA', 'Ana Pérez',
     'ventas@medsur.cl', '+56222223333', 'Av. Siempre Viva 123', 'Santiago', 'admin');

CALL sp_producto_insertar('GUA-001', 'Guantes de nitrilo talla M', 'Caja 100 unidades',
     1, 1, 4500, 6990, 20, NULL, 0, 'admin');
CALL sp_producto_insertar('PAR-500', 'Paracetamol 500 mg', 'Caja 16 comprimidos',
     2, 1, 800, 1490, 50, '2027-12-31', 0, 'admin');

CALL sp_movimiento_registrar(1, 'ENTRADA', 100, 'Compra inicial', 'FAC-1001', 'admin');
CALL sp_movimiento_registrar(2, 'ENTRADA', 200, 'Compra inicial', 'FAC-1001', 'admin');
CALL sp_movimiento_registrar(1, 'SALIDA', 15, 'Despacho a clínica', 'GD-2001', 'admin');

SELECT * FROM vw_productodetalle;
