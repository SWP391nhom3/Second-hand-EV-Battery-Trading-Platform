.PHONY: help build up down restart logs clean migrate migration

help: ## Show this help message
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Available targets:'
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  %-15s %s\n", $$1, $$2}' $(MAKEFILE_LIST)

build: ## Build Docker images
	docker-compose build

up: ## Start all services
	docker-compose up -d

down: ## Stop all services
	docker-compose down

restart: ## Restart all services
	docker-compose restart

logs: ## Show logs from all services
	docker-compose logs -f

logs-api: ## Show logs from API service
	docker-compose logs -f api

logs-db: ## Show logs from SQL Server service
	docker-compose logs -f sqlserver

clean: ## Stop services and remove volumes
	docker-compose down -v

migrate: ## Run database migrations
	dotnet ef database update --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API

migration: ## Create a new migration (usage: make migration NAME=MigrationName)
	@if [ -z "$(NAME)" ]; then \
		echo "Error: Migration name is required"; \
		echo "Usage: make migration NAME=MigrationName"; \
		exit 1; \
	fi
	dotnet ef migrations add $(NAME) --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API

migration-list: ## List all migrations
	dotnet ef migrations list --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API

migration-script: ## Generate SQL script for migrations (usage: make migration-script OUTPUT=script.sql)
	@if [ -z "$(OUTPUT)" ]; then \
		dotnet ef migrations script --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API; \
	else \
		dotnet ef migrations script --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API --output $(OUTPUT); \
	fi

migration-remove: ## Remove the last migration (usage: make migration-remove)
	dotnet ef migrations remove --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API

restore: ## Restore NuGet packages
	dotnet restore

build-local: ## Build solution locally
	dotnet build

run-local: ## Run API locally
	dotnet run --project src/EVehicle.API

test: ## Run tests
	dotnet test

