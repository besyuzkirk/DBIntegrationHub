#!/bin/bash
# Database restore script

set -e

if [ -z "$1" ]; then
    echo "❌ Kullanım: ./restore.sh <backup_file.sql.gz>"
    echo ""
    echo "Mevcut backup'lar:"
    ls -lh ./backups/*.sql.gz 2>/dev/null || echo "Backup bulunamadı"
    exit 1
fi

BACKUP_FILE=$1

if [ ! -f "$BACKUP_FILE" ]; then
    echo "❌ Hata: Dosya bulunamadı: $BACKUP_FILE"
    exit 1
fi

echo "⚠️  DİKKAT: Mevcut database silinecek ve backup geri yüklenecek!"
echo "Devam etmek için ENTER'a basın, iptal için CTRL+C..."
read

# Eğer .gz dosyası ise önce extract et
if [[ $BACKUP_FILE == *.gz ]]; then
    echo "🔵 Backup dosyası extract ediliyor..."
    gunzip -k "$BACKUP_FILE"
    SQL_FILE="${BACKUP_FILE%.gz}"
else
    SQL_FILE="$BACKUP_FILE"
fi

echo "🔵 Database restore ediliyor..."

# Restore işlemi
docker-compose exec -T postgres psql -U postgres -d DBIntegrationHub < "$SQL_FILE"

# Extract edilmiş geçici dosyayı temizle
if [[ $BACKUP_FILE == *.gz ]] && [ -f "$SQL_FILE" ]; then
    rm "$SQL_FILE"
fi

echo "✅ Restore tamamlandı!"

