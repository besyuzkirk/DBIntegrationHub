# DBIntegrationHub - Makefile
# Kısa komutlar için yardımcı dosya

.PHONY: help build up down restart logs clean dev-up dev-down prod-up prod-down

# Varsayılan hedef
.DEFAULT_GOAL := help

# Renkli output için
BLUE := \033[0;34m
GREEN := \033[0;32m
YELLOW := \033[0;33m
RED := \033[0;31m
NC := \033[0m # No Color

help: ## Yardım menüsünü gösterir
	@echo "$(BLUE)DBIntegrationHub - Docker Komutları$(NC)"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "$(GREEN)%-20s$(NC) %s\n", $$1, $$2}'
	@echo ""

# Production komutları
build: ## Tüm servisleri build et
	@echo "$(BLUE)Build ediliyor...$(NC)"
	docker-compose build --no-cache

up: ## Production ortamını başlat
	@echo "$(GREEN)Production ortamı başlatılıyor...$(NC)"
	docker-compose up -d
	@echo "$(GREEN)✓ Servisler başlatıldı!$(NC)"
	@make status

down: ## Production ortamını durdur
	@echo "$(YELLOW)Production ortamı durduruluyor...$(NC)"
	docker-compose down
	@echo "$(GREEN)✓ Servisler durduruldu!$(NC)"

restart: ## Production ortamını yeniden başlat
	@echo "$(YELLOW)Production ortamı yeniden başlatılıyor...$(NC)"
	docker-compose restart
	@echo "$(GREEN)✓ Servisler yeniden başlatıldı!$(NC)"

# Development komutları
dev-up: ## Development ortamını başlat (sadece PostgreSQL + pgAdmin)
	@echo "$(GREEN)Development ortamı başlatılıyor...$(NC)"
	docker-compose -f docker-compose.dev.yml up -d
	@echo "$(GREEN)✓ Development servisleri başlatıldı!$(NC)"
	@echo "$(BLUE)PostgreSQL: localhost:5432$(NC)"
	@echo "$(BLUE)pgAdmin: http://localhost:5050$(NC)"

dev-down: ## Development ortamını durdur
	@echo "$(YELLOW)Development ortamı durduruluyor...$(NC)"
	docker-compose -f docker-compose.dev.yml down
	@echo "$(GREEN)✓ Development servisleri durduruldu!$(NC)"

# Log komutları
logs: ## Tüm servislerin loglarını göster
	docker-compose logs -f

logs-api: ## API loglarını göster
	docker-compose logs -f api

logs-ui: ## UI loglarını göster
	docker-compose logs -f ui

logs-db: ## Database loglarını göster
	docker-compose logs -f postgres

# Status komutları
status: ## Servislerin durumunu göster
	@echo "$(BLUE)Servis Durumu:$(NC)"
	@docker-compose ps
	@echo ""
	@echo "$(BLUE)Erişim Bilgileri:$(NC)"
	@echo "$(GREEN)Frontend:  http://localhost:3000$(NC)"
	@echo "$(GREEN)API:       http://localhost:5149$(NC)"
	@echo "$(GREEN)Swagger:   http://localhost:5149/swagger$(NC)"
	@echo "$(GREEN)PostgreSQL: localhost:5432$(NC)"

health: ## Health check yap
	@echo "$(BLUE)Health Check...$(NC)"
	@echo -n "API: "
	@curl -s http://localhost:5149/health > /dev/null && echo "$(GREEN)✓ OK$(NC)" || echo "$(RED)✗ FAIL$(NC)"

# Database komutları
db-shell: ## PostgreSQL shell'e bağlan
	docker-compose exec postgres psql -U postgres -d DBIntegrationHub

db-backup: ## Database backup al
	@echo "$(BLUE)Backup alınıyor...$(NC)"
	@mkdir -p backups
	docker-compose exec postgres pg_dump -U postgres DBIntegrationHub > backups/backup_$(shell date +%Y%m%d_%H%M%S).sql
	@echo "$(GREEN)✓ Backup tamamlandı!$(NC)"

db-restore: ## Database backup'ı geri yükle (kullanım: make db-restore FILE=backup.sql)
	@echo "$(YELLOW)Backup geri yükleniyor: $(FILE)$(NC)"
	docker-compose exec -T postgres psql -U postgres DBIntegrationHub < $(FILE)
	@echo "$(GREEN)✓ Restore tamamlandı!$(NC)"

db-reset: ## Database'i sıfırla (DİKKAT!)
	@echo "$(RED)⚠ Database silinecek! Devam etmek için CTRL+C ile iptal edin$(NC)"
	@sleep 5
	docker-compose down -v postgres
	docker-compose up -d postgres
	@echo "$(GREEN)✓ Database sıfırlandı!$(NC)"

# Cleanup komutları
clean: ## Tüm container, volume ve image'ları temizle
	@echo "$(RED)⚠ Tüm veriler silinecek! Devam etmek için CTRL+C ile iptal edin$(NC)"
	@sleep 5
	docker-compose down -v
	docker system prune -af --volumes
	@echo "$(GREEN)✓ Temizleme tamamlandı!$(NC)"

clean-volumes: ## Sadece volume'leri temizle
	@echo "$(YELLOW)Volume'ler temizleniyor...$(NC)"
	docker-compose down -v
	@echo "$(GREEN)✓ Volume'ler temizlendi!$(NC)"

# Build ve başlat komutları
fresh: ## Temiz build ve başlat
	@echo "$(BLUE)Temiz build yapılıyor...$(NC)"
	@make down
	@make build
	@make up
	@echo "$(GREEN)✓ Fresh start tamamlandı!$(NC)"

# Test komutları
test: ## Servisleri test et
	@echo "$(BLUE)Test ediliyor...$(NC)"
	@make health
	@echo "$(BLUE)Container durumu:$(NC)"
	@docker-compose ps

# Production deployment
deploy: ## Production'a deploy et
	@echo "$(BLUE)Production deployment başlıyor...$(NC)"
	@make build
	@make down
	@make up
	@sleep 10
	@make health
	@echo "$(GREEN)✓ Deployment tamamlandı!$(NC)"

