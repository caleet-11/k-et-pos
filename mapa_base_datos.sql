-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: localhost    Database: puntodeventadb
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `caja_turnos`
--

DROP TABLE IF EXISTS `caja_turnos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `caja_turnos` (
  `IdTurno` int NOT NULL AUTO_INCREMENT,
  `IdUsuario` int NOT NULL,
  `MontoInicial` decimal(10,2) NOT NULL,
  `FechaApertura` datetime NOT NULL,
  `FechaCierre` datetime DEFAULT NULL,
  `MontoCierre` decimal(10,2) DEFAULT NULL,
  `EstadoActivo` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`IdTurno`),
  KEY `IX_CajaTurnos_Usuario_Estado` (`IdUsuario`,`EstadoActivo`),
  CONSTRAINT `caja_turnos_ibfk_1` FOREIGN KEY (`IdUsuario`) REFERENCES `usuarios` (`IdUsuario`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `clientes`
--

DROP TABLE IF EXISTS `clientes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clientes` (
  `IdCliente` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(150) NOT NULL,
  `Telefono` varchar(20) DEFAULT NULL,
  `LimiteCredito` decimal(10,2) DEFAULT '0.00',
  `DeudaActual` decimal(10,2) DEFAULT '0.00',
  `Activo` tinyint(1) DEFAULT '1',
  `CorreoElectronico` varchar(100) DEFAULT NULL,
  `RFC` varchar(15) DEFAULT NULL,
  `Direccion` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`IdCliente`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `devoluciones`
--

DROP TABLE IF EXISTS `devoluciones`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `devoluciones` (
  `IdDevolucion` int NOT NULL AUTO_INCREMENT,
  `IdDetalle` int NOT NULL,
  `Codigo` varchar(50) NOT NULL,
  `Cantidad` decimal(10,3) NOT NULL,
  `MontoReembolsado` decimal(10,2) NOT NULL,
  `FechaHora` datetime NOT NULL,
  `IdUsuario` int NOT NULL,
  `FolioVenta` varchar(50) NOT NULL,
  PRIMARY KEY (`IdDevolucion`),
  KEY `IdDetalle` (`IdDetalle`),
  KEY `IdUsuario` (`IdUsuario`),
  CONSTRAINT `devoluciones_ibfk_1` FOREIGN KEY (`IdDetalle`) REFERENCES `ventas_detalles` (`IdDetalle`),
  CONSTRAINT `devoluciones_ibfk_2` FOREIGN KEY (`IdUsuario`) REFERENCES `usuarios` (`IdUsuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `productos`
--

DROP TABLE IF EXISTS `productos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `productos` (
  `Codigo` varchar(50) NOT NULL,
  `Nombre` varchar(150) NOT NULL,
  `Marca` varchar(50) DEFAULT NULL,
  `Proveedor` varchar(100) DEFAULT NULL,
  `PrecioCosto` decimal(10,2) NOT NULL DEFAULT '0.00',
  `PrecioVenta` decimal(10,2) NOT NULL DEFAULT '0.00',
  `Stock` decimal(10,3) NOT NULL DEFAULT '0.000',
  `StockMinimo` decimal(10,3) NOT NULL DEFAULT '5.000',
  `StockIdeal` decimal(10,3) NOT NULL DEFAULT '10.000',
  `ControlaStock` tinyint(1) NOT NULL DEFAULT '1',
  `SeVendePorUnidad` tinyint(1) NOT NULL DEFAULT '1',
  `Activo` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Codigo`),
  FULLTEXT KEY `FX_Productos_Busqueda` (`Nombre`,`Codigo`,`Marca`,`Proveedor`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `proveedores`
--

DROP TABLE IF EXISTS `proveedores`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `proveedores` (
  `IdProveedor` int NOT NULL AUTO_INCREMENT,
  `Empresa` varchar(100) NOT NULL,
  `NombreContacto` varchar(100) DEFAULT NULL,
  `Telefono` varchar(20) DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IdProveedor`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `IdRol` int NOT NULL AUTO_INCREMENT,
  `NombreRol` varchar(50) NOT NULL,
  PRIMARY KEY (`IdRol`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `IdUsuario` int NOT NULL AUTO_INCREMENT,
  `IdRol` int NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `NombreUsuario` varchar(50) NOT NULL,
  `Contrasena` varchar(255) NOT NULL,
  `Activo` tinyint(1) DEFAULT '1',
  `IntentosFallidos` int DEFAULT '0',
  `Bloqueado` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`IdUsuario`),
  UNIQUE KEY `NombreUsuario` (`NombreUsuario`),
  KEY `IdRol` (`IdRol`),
  CONSTRAINT `usuarios_ibfk_1` FOREIGN KEY (`IdRol`) REFERENCES `roles` (`IdRol`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ventas`
--

DROP TABLE IF EXISTS `ventas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ventas` (
  `IdVenta` int NOT NULL AUTO_INCREMENT,
  `Folio` varchar(50) NOT NULL,
  `FechaHora` datetime NOT NULL,
  `Total` decimal(10,2) NOT NULL,
  `IdUsuario` int NOT NULL,
  `TipoOperacion` varchar(20) NOT NULL DEFAULT 'Efectivo',
  `IdCliente` int DEFAULT NULL,
  `EstadoPago` varchar(20) NOT NULL DEFAULT 'Pagado',
  PRIMARY KEY (`IdVenta`),
  UNIQUE KEY `Folio` (`Folio`),
  KEY `IdUsuario` (`IdUsuario`),
  KEY `IdCliente` (`IdCliente`),
  CONSTRAINT `ventas_ibfk_1` FOREIGN KEY (`IdUsuario`) REFERENCES `usuarios` (`IdUsuario`),
  CONSTRAINT `ventas_ibfk_2` FOREIGN KEY (`IdCliente`) REFERENCES `clientes` (`IdCliente`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ventas_detalles`
--

DROP TABLE IF EXISTS `ventas_detalles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ventas_detalles` (
  `IdDetalle` int NOT NULL AUTO_INCREMENT,
  `IdVenta` int NOT NULL,
  `Codigo` varchar(50) NOT NULL,
  `Nombre` varchar(150) NOT NULL,
  `Cantidad` decimal(10,3) NOT NULL,
  `PrecioCosto` decimal(10,2) NOT NULL,
  `PrecioVenta` decimal(10,2) NOT NULL,
  `Subtotal` decimal(10,2) NOT NULL,
  `FueDevuelto` tinyint(1) DEFAULT '0',
  `CantidadDevuelta` decimal(10,3) DEFAULT '0.000',
  PRIMARY KEY (`IdDetalle`),
  KEY `IdVenta` (`IdVenta`),
  KEY `Codigo` (`Codigo`),
  CONSTRAINT `ventas_detalles_ibfk_1` FOREIGN KEY (`IdVenta`) REFERENCES `ventas` (`IdVenta`) ON DELETE CASCADE,
  CONSTRAINT `ventas_detalles_ibfk_2` FOREIGN KEY (`Codigo`) REFERENCES `productos` (`Codigo`) ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-08-22 17:35:37
