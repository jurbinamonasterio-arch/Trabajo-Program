-- =====================================================================
-- SIGBIM - Usuario de aplicación con mínimo privilegio
-- Control ISO/IEC 27001:2022 A.8.2 (accesos privilegiados) y
-- A.8.3 (restricción de acceso a la información).
--
-- La aplicación C# NO se conecta como root: usa este usuario, que solo
-- puede EJECUTAR procedimientos almacenados. No puede hacer SELECT,
-- INSERT, UPDATE ni DELETE directo sobre las tablas, por lo que toda
-- manipulación de datos pasa obligatoriamente por los SPs.
--
-- IMPORTANTE: cambia la contraseña antes de usarlo y NO la subas a GitHub.
-- =====================================================================

-- Se elimina también una posible cuenta antigua en 'localhost': si existiera,
-- MySQL la preferiría sobre la de '%' (es más específica) y rechazaría la clave.
DROP USER IF EXISTS 'sigbim_app'@'localhost';
DROP USER IF EXISTS 'sigbim_app'@'%';
CREATE USER 'sigbim_app'@'%' IDENTIFIED BY 'CambiarEstaClave#2026';

-- Solo permiso de ejecución sobre los procedimientos del esquema
GRANT EXECUTE ON sigbim_mac.* TO 'sigbim_app'@'%';

FLUSH PRIVILEGES;

-- Verificación
SHOW GRANTS FOR 'sigbim_app'@'%';
