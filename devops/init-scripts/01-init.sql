-- DBIntegrationHub veritabanı başlangıç scripti
-- Bu script container ilk kez başlatıldığında otomatik olarak çalışır

-- Database oluşturma (zaten docker-compose'da POSTGRES_DB ile oluşturuluyor)
-- SELECT 'CREATE DATABASE DBIntegrationHub' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'DBIntegrationHub')\gexec

-- Gerekirse ek tablolar veya seed data buraya eklenebilir
-- Örneğin: default connection tanımları, test verileri vb.

\c DBIntegrationHub;

-- Extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- İstatistik için view (opsiyonel)
CREATE OR REPLACE VIEW database_stats AS
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- Başarılı mesaj
SELECT 'DBIntegrationHub veritabanı başarıyla hazırlandı!' as message;

