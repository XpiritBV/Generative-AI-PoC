resource "generative-ai-poc-azurerm_resource_group" "rg" {
  location = var.resource_group_location
  name     = "rg-generative-ai-poc"
}

resource "azurerm_storage_account" "terraform" {
  name                     = "terraformpersistance"
  resource_group_name      = generative-ai-poc-azurerm_resource_group.terraform.name
  location                 = generative-ai-poc-azurerm_resource_group.terraform.location
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