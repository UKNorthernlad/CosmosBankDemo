# Variables
$initials="def" # change to your initials
$resourceGroupName="demo2"
$location="uksouth"
$MANAGED_IDENTITY_NAME = "acabatchdemo"

# Cosmos DB
$accountName="cosmosdb-$initials"
$databaseName="Bank01"
$containerName="Accounts"
$partitionKeyPath="/account"
$ruLimit=10000
$ttl=600 # TTL in seconds (10 minutes)

# Container Registry
$registryName="acr$initials"
$registryNameFQDN="$registryName.azurecr.io"

# Container Apps Environment
$ENVIRONMENT="myEnvironment"
$containerapp="batchdemo"
$image="cosmos-batch-insertion:latest"

# Application
$COSMOS_URL="https://cosmosdb-def.documents.azure.com:443"
$DATABASE_NAME="Bank01"
$CONTAINER_NAME="Accounts"
$RECORD_QUANTITY=1000000
$BATCH_SIZE=100

# Create a resource group
az group create --name $resourceGroupName --location $location

# Create a managed identity
#az identity create --name $MANAGED_IDENTITY_NAME --resource-group $resourceGroupName
#$MANAGED_IDENTITY_CLIENT_ID=$(az identity show --name $MANAGED_IDENTITY_NAME --resource-group $resourceGroupName --query 'clientId' -o tsv)

# Create a CosmosDB account
az cosmosdb create --name $accountName --resource-group $resourceGroupName --locations regionName=$location failoverPriority=0 isZoneRedundant=False

# Create a database
az cosmosdb sql database create --account-name $accountName --resource-group $resourceGroupName --name $databaseName

# Create a container with RU limit and TTL
# az cosmosdb sql container create --account-name $accountName --resource-group $resourceGroupName --database-name $databaseName --name $containerName --partition-key-path $partitionKeyPath --throughput $ruLimit --ttl $ttl

# Create container registry
az acr create --resource-group $resourceGroupName --name $registryName --sku Basic
az acr update -n $registryName --admin-enabled true
#az role assignment create --assignee $MANAGED_IDENTITY_CLIENT_ID --role "AcrPull" --scope $(az acr show --name $registryName --query id --output tsv)

# Docker Build
docker build -t cosmos-batch-insertion .
docker tag cosmos-batch-insertion:latest acrdef.azurecr.io/cosmos-batch-insertion:latest
az acr login --name $registryName
docker push acrdef.azurecr.io/cosmos-batch-insertion:latest

# CosmosDB keys
$key = az cosmosdb keys list --name $accountName --resource-group $resourceGroupName --type keys --query primaryMasterKey -o tsv

##
# Create Azure Container App
##
az containerapp env create --name $ENVIRONMENT --resource-group $resourceGroupName --location $location

# Create the container app
az containerapp create --name $containerapp --resource-group $resourceGroupName --environment $ENVIRONMENT --registry-identity system --registry-server $registryNameFQDN --image $registryNameFQDN/$image --env-vars COSMOS_URL=$COSMOS_URL COSMOS_KEY=$key DATABASE_NAME=$DATABASE_NAME CONTAINER_NAME=$CONTAINER_NAME RECORD_QUANTITY=$RECORD_QUANTITY BATCH_SIZE=$BATCH_SIZE

