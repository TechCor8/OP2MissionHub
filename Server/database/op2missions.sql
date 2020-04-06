-- phpMyAdmin SQL Dump
-- version 4.8.5
-- https://www.phpmyadmin.net/
--
-- Host: mysqlhost.com
-- Generation Time: Apr 05, 2020 at 05:21 PM
-- Server version: 5.7.28-log
-- PHP Version: 7.1.22

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `op2missions`
--

-- --------------------------------------------------------

--
-- Table structure for table `AppConstants`
--

CREATE TABLE `AppConstants` (
  `AppConstantID` int(10) UNSIGNED NOT NULL,
  `ConstantName` varchar(32) NOT NULL,
  `ConstantValue` varchar(128) NOT NULL,
  `AdminLock` tinyint(3) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `AppConstants`
--

INSERT INTO `AppConstants` (`AppConstantID`, `ConstantName`, `ConstantValue`, `AdminLock`) VALUES
(1, 'APP_VERSION', '1.0', 1),
(2, 'MAINTENANCE_MODE', '0', 1),
(3, 'MAINTENANCE_MODE_LOGIN_ACCESS', '1', 1),
(4, 'MAX_DAILY_ACTIVE_USERS', '5', 1),
(5, 'DAYS_UNTIL_USER_DELETED', '365', 1),
(6, 'CREATE_ACCOUNT_IP_LIMIT', '4', 1),
(7, 'FORGOT_PASSWORD_TIME_DELAY', '7200', 1),
(8, 'ASSET_DOWNLOAD_URL', '', 0),
(9, 'APP_NAME', 'OP2 Mission Hub', 0),
(10, 'APP_URL', 'https://github.com/TechCor8/OP2MissionHub', 0),
(11, 'PUBLISHER_NAME', 'krealm', 0),
(12, 'PUBLISHER_URL', 'https://krealm.xyz', 0),
(13, 'PUBLISHER_EMAIL', 'support@krealm.xyz', 0);

-- --------------------------------------------------------

--
-- Table structure for table `AuthTypes`
--

CREATE TABLE `AuthTypes` (
  `AuthTypeID` tinyint(3) UNSIGNED NOT NULL,
  `InternalName` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `AuthTypes`
--

INSERT INTO `AuthTypes` (`AuthTypeID`, `InternalName`) VALUES
(1, 'Custom');

-- --------------------------------------------------------

--
-- Table structure for table `BannedNames`
--

CREATE TABLE `BannedNames` (
  `BannedNameID` int(10) UNSIGNED NOT NULL,
  `BannedName` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `BannedNames`
--

INSERT INTO `BannedNames` (`BannedNameID`, `BannedName`) VALUES
(1, 'admin'),
(2, 'administrator'),
(3, 'developer'),
(4, 'server');

-- --------------------------------------------------------

--
-- Table structure for table `BanTypes`
--

CREATE TABLE `BanTypes` (
  `BanTypeID` tinyint(3) UNSIGNED NOT NULL,
  `InternalName` varchar(16) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `BanTypes`
--

INSERT INTO `BanTypes` (`BanTypeID`, `InternalName`) VALUES
(1, 'Reserved'),
(2, 'Transactions'),
(3, 'Reserved'),
(4, 'Login');

-- --------------------------------------------------------

--
-- Table structure for table `MissionFiles`
--

CREATE TABLE `MissionFiles` (
  `MissionFileID` int(10) UNSIGNED NOT NULL,
  `MissionID` int(10) UNSIGNED NOT NULL,
  `FileName` varchar(64) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Table structure for table `Missions`
--

CREATE TABLE `Missions` (
  `MissionID` int(10) UNSIGNED NOT NULL,
  `CreatedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `MissionName` varchar(64) NOT NULL,
  `MissionDescription` varchar(512) NOT NULL,
  `GitHubLink` varchar(256) NOT NULL,
  `AuthorID` int(10) UNSIGNED NOT NULL,
  `CertifyingAdminID` int(10) UNSIGNED DEFAULT NULL,
  `Version` smallint(5) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Table structure for table `Users`
--

CREATE TABLE `Users` (
  `UserID` int(10) UNSIGNED NOT NULL,
  `AuthTypeID` tinyint(3) UNSIGNED NOT NULL,
  `UserName` varchar(32) NOT NULL,
  `DisplayName` varchar(32) NOT NULL,
  `Password` varchar(255) DEFAULT NULL,
  `Email` varchar(255) DEFAULT NULL,
  `EmailToken` varchar(255) DEFAULT NULL,
  `EmailVerified` tinyint(4) NOT NULL DEFAULT '0',
  `SessionToken` varchar(255) DEFAULT NULL,
  `SessionLastUpdated` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `RememberMeToken` varchar(255) DEFAULT NULL,
  `LastLoginTime` timestamp NOT NULL,
  `LastLoginAttempt` timestamp NOT NULL,
  `FailedLoginAttempts` mediumint(8) UNSIGNED NOT NULL DEFAULT '0',
  `LogOutTime` timestamp NULL DEFAULT NULL,
  `ResetPasswordToken` varchar(255) DEFAULT NULL,
  `ResetPasswordTime` timestamp NULL DEFAULT NULL,
  `BanTypeID` tinyint(3) UNSIGNED DEFAULT NULL,
  `BanStartTime` timestamp NULL DEFAULT NULL,
  `BanEndTime` timestamp NULL DEFAULT NULL,
  `BanCount` mediumint(9) NOT NULL DEFAULT '0',
  `BanReason` varchar(128) NOT NULL,
  `CreationDate` timestamp NOT NULL,
  `CreatedIPAddress` varbinary(16) NOT NULL,
  `IsAdmin` tinyint(1) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `AppConstants`
--
ALTER TABLE `AppConstants`
  ADD PRIMARY KEY (`AppConstantID`);

--
-- Indexes for table `AuthTypes`
--
ALTER TABLE `AuthTypes`
  ADD PRIMARY KEY (`AuthTypeID`);

--
-- Indexes for table `BannedNames`
--
ALTER TABLE `BannedNames`
  ADD PRIMARY KEY (`BannedNameID`);

--
-- Indexes for table `BanTypes`
--
ALTER TABLE `BanTypes`
  ADD PRIMARY KEY (`BanTypeID`);

--
-- Indexes for table `MissionFiles`
--
ALTER TABLE `MissionFiles`
  ADD PRIMARY KEY (`MissionFileID`),
  ADD UNIQUE KEY `MissionFile` (`MissionID`,`FileName`) USING BTREE,
  ADD UNIQUE KEY `FileName` (`FileName`);

--
-- Indexes for table `Missions`
--
ALTER TABLE `Missions`
  ADD PRIMARY KEY (`MissionID`),
  ADD KEY `AuthorID` (`AuthorID`),
  ADD KEY `CertifyingAdminID` (`CertifyingAdminID`);

--
-- Indexes for table `Users`
--
ALTER TABLE `Users`
  ADD PRIMARY KEY (`UserID`),
  ADD UNIQUE KEY `DisplayName` (`DisplayName`) USING BTREE,
  ADD UNIQUE KEY `UserName` (`AuthTypeID`,`UserName`) USING BTREE,
  ADD UNIQUE KEY `Email` (`Email`) USING BTREE,
  ADD KEY `AuthType` (`AuthTypeID`) USING BTREE;

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `AppConstants`
--
ALTER TABLE `AppConstants`
  MODIFY `AppConstantID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14;

--
-- AUTO_INCREMENT for table `BannedNames`
--
ALTER TABLE `BannedNames`
  MODIFY `BannedNameID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT for table `MissionFiles`
--
ALTER TABLE `MissionFiles`
  MODIFY `MissionFileID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `Missions`
--
ALTER TABLE `Missions`
  MODIFY `MissionID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `Users`
--
ALTER TABLE `Users`
  MODIFY `UserID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `MissionFiles`
--
ALTER TABLE `MissionFiles`
  ADD CONSTRAINT `MissionFiles_MissionID` FOREIGN KEY (`MissionID`) REFERENCES `Missions` (`MissionID`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `Missions`
--
ALTER TABLE `Missions`
  ADD CONSTRAINT `Missions_AuthorID` FOREIGN KEY (`AuthorID`) REFERENCES `Users` (`UserID`),
  ADD CONSTRAINT `Missions_CertifyingAdminID` FOREIGN KEY (`CertifyingAdminID`) REFERENCES `Users` (`UserID`);

--
-- Constraints for table `Users`
--
ALTER TABLE `Users`
  ADD CONSTRAINT `AuthTypeID` FOREIGN KEY (`AuthTypeID`) REFERENCES `AuthTypes` (`AuthTypeID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
