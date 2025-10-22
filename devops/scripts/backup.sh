#!/bin/bash
# Database backup script

set -e

BACKUP_DIR="./backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/dbintegrationhub_$TIMESTAMP.sql"

# Backup dizinini oluştur
mkdir -p "$BACKUP_DIR"

echo "🔵 Backup başlatılıyor..."

# Backup al
docker-compose exec -T postgres pg_dump -U postgres DBIntegrationHub > "$BACKUP_FILE"

# Compress et
gzip "$BACKUP_FILE"

echo "✅ Backup tamamlandı: ${BACKUP_FILE}.gz"

# Eski backup'ları temizle (30 günden eski olanlar)
find "$BACKUP_DIR" -name "*.sql.gz" -mtime +30 -delete

echo "📊 Mevcut backup'lar:"
ls -lh "$BACKUP_DIR"

