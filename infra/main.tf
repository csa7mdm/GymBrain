terraform {
  required_version = ">= 1.0.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

# Resource Group
resource "azurerm_resource_group" "gymbrain" {
  name     = "rg-gymbrain-${var.environment}"
  location = var.location
  tags = var.tags
}

# PostgreSQL Flexible Server (Neon equivalent on Azure)
resource "azurerm_postgresql_flexible_server" "gymbrain_db" {
  name                = "psql-gymbrain-${var.environment}"
  resource_group_name = azurerm_resource_group.gymbrain.name
  location            = azurerm_resource_group.gymbrain.location
  administrator_login = var.db_admin_username
  administrator_login_password = var.db_admin_password
  version             = "15"
  sku_name            = "B_BMS"   # Burstable, 1 vCore, 2 GiB RAM
  storage_mb          = 32768     # 32 GB
  backup_retention_days = 7
  geo_redoundant_backup_enabled = false
  public_network_access_enabled = true  # For demo; in production use private endpoints
  delegation_subnet_id = azurerm_subnet.db_subnet.id

  tags = var.tags
}

# PostgreSQL Database
resource "azurerm_postgresql_flexible_server_database" "gymbrain" {
  name                = "gymbrain"
  resource_group_name = azurerm_resource_group.gymbrain.name
  server_name         = azurerm_postgresql_flexible_server.gymbrain_db.name
  charset             = "UTF8"
  collation           = "en_US.utf8"
}

# Redis Cache (Upstash equivalent on Azure)
resource "azurerm_redis_cache" "gymbrain_cache" {
  name                = "redis-gymbrain-${var.environment}"
  location            = azurerm_resource_group.gymbrain.location
  resource_group_name = azurerm_resource_group.gymbrain.name
  redis_version       = "6"
  sku_name            = "Basic"
  sku_capacity        = 1   # 250 MB
  enable_non_ssl_port = false
  minimum_tls_version = "1.2"
  subnet_id           = azurerm_subnet.cache_subnet.id

  tags = var.tags
}

# Virtual Network for service delegation
resource "azurerm_virtual_network" "gymbrain_vnet" {
  name                = "vnet-gymbrain-${var.environment}"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.gymbrain.location
  resource_group_name = azurerm_resource_group.gymbrain.name
  tags                = var.tags
}

# Subnets
resource "azurerm_subnet" "db_subnet" {
  name                 = "snet-db"
  resource_group_name  = azurerm_resource_group.gymbrain.name
  virtual_network_name = azurerm_virtual_network.gymbrain_vnet.name
  address_prefixes     = ["10.0.1.0/24"]
  delegation {
    name = "postgresqlDelegation"
    service_delegation {
      name    = "Microsoft.DBforPostgreSQL/flexibleServers"
      actions = ["Microsoft.Network/virtualNetworks/subnets/prepareServiceAction"]
    }
  }
}

resource "azurerm_subnet" "cache_subnet" {
  name                 = "snet-cache"
  resource_group_name  = azurerm_resource_group.gymbrain.name
  virtual_network_name = azurerm_virtual_network.gymbrain_vnet.name
  address_prefixes     = ["10.0.2.0/24"]
  delegation {
    name = "redisDelegation"
    service_delegation {
      name    = "Microsoft.Cache/redisEnterprise"
      actions = ["Microsoft.Network/virtualNetworks/subnets/prepareServiceAction"]
    }
  }
}

# App Service Plan for API
resource "azurerm_app_service_plan" "gymbrain_api" {
  name                = "asp-gymbrain-${var.environment}"
  location            = azurerm_resource_group.gymbrain.location
  resource_group_name = azurerm_resource_group.gymbrain.name
  kind                = "Linux"
  reserved            = true

  sku {
    tier = "Standard"
    size = "S1"
  }

  tags = var.tags
}

# Web App for API
resource "azurerm_app_service" "gymbrain_api" {
  name                = "app-gymbrain-${var.environment}"
  location            = azurerm_resource_group.gymbrain.location
  resource_group_name = azurerm_resource_group.gymbrain.name
  app_service_plan_id = azurerm_app_service_plan.gymbrain_api.id

  site_config {
    linux_fx_version = "DOTNET|9.0"
    application_stack {
      dot_net_version = "v9.0"
    }
    always_on = true
    ftps_state = "Disabled"
  }

  app_settings = {
    "ConnectionStrings__DefaultConnection" = azurerm_postgresql_flexible_server.gymbrain_db.connection_string
    "REDIS_CONNECTION"                     = "${azurerm_redis_cache.gymbrain_cache.host_name}:6380,password=${azurerm_redis_cache.gymbrain_cache.primary_access_key},ssl=True,abortConnect=False"
    "Jwt__Secret"                          = var.jwt_secret
    "Jwt__Issuer"                          = "GymBrain"
    "Jwt__Audience"                        = "GymBrain"
    "ASPNETCORE_ENVIRONMENT"               = var.environment
  }

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Export outputs
output "api_endpoint" {
  description = "The HTTP endpoint of the GymBrain API"
  value       = azurerm_app_service.gymbrain_api.default_site_hostname
}

output "db_connection_string" {
  description = "Connection string for the PostgreSQL database"
  value       = azurerm_postgresql_flexible_server.gymbrain_db.connection_string
  sensitive   = true
}

output "redis_connection_string" {
  description = "Connection string for the Redis cache"
  value       = "${azurerm_redis_cache.gymbrain_cache.host_name}:6380,password=${azurerm_redis_cache.gymbrain_cache.primary_access_key},ssl=True,abortConnect=False"
  sensitive   = true
}