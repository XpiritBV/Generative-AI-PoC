locals {
  tags = {
    owner = "Jeroen van de Kraats"
  }
}

resource "azurerm_resource_group" "resourcegroup" {
  name     = var.resource_group_name
  location = var.location
  tags = local.tags
}