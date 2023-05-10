resource "generative-ai-poc-azurerm_resource_group" "rg" {
  location = var.location
  name     = var.resource_group_name
}

resource "azurerm_storage_account" "terraform" {
  name                     = "terraformpersistance"
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = {
    environment = "terraform"
  }
}

terraform {
  backend "azurerm" {
    storage_account_name = "terraformpersistance"
    container_name       = "terraform-state"
    key                  = "terraform.tfstate"
  }
}