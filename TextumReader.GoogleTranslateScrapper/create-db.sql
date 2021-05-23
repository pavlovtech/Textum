USE master ;  
GO  
CREATE DATABASE HangfireGoogleTranslateScrapper  
ON   
( NAME = Sales_dat,  
    FILENAME = 'G:\hangfire\hangfire.mdf',  
    SIZE = 10,  
    MAXSIZE = 50,  
    FILEGROWTH = 5 )  
LOG ON  
( NAME = Sales_log,  
    FILENAME = 'G:\hangfire\hangfir.ldf',  
    SIZE = 5MB,  
    MAXSIZE = 25MB,  
    FILEGROWTH = 5MB ) ;  
GO  