# Cloudy Weather

Deployment of the CloudyWeather template to **Azure App Service** (Part 1) and the same functionality as an **HTTP-triggered Azure Function** (Part 2).

All commands are PowerShell and assume a fresh Azure account with no prior services set up.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- Azure Functions Core Tools (Part 2 only):
  ```powershell
  winget install Microsoft.Azure.FunctionsCoreTools
  ```
- An Azure subscription (e.g. Azure for Students)

## Setup (run once)

Log in and register the resource providers a new subscription needs:

```powershell
az login
az provider register --namespace Microsoft.Web --wait
az provider register --namespace Microsoft.Storage --wait
az provider register --namespace Microsoft.Insights --wait
```

---

## Part 1 – Azure App Service

Run from the `CloudyWeather.API` folder.

### 1. Variables

```powershell
$RG = "cloudy-weather-is"
$LOCATION = "norwayeast"
$PLAN = "cloudy-weather-plan"
$APP = "cloudy-weather-bb6767"   # must be globally unique
```

### 2. Create resource group, App Service plan and web app

```powershell
az group create --name $RG --location $LOCATION
az appservice plan create --name $PLAN --resource-group $RG --sku F1 --is-linux
az webapp create --name $APP --resource-group $RG --plan $PLAN --runtime "DOTNETCORE:10.0"
```

### 3. Set environment variable

```powershell
az webapp config appsettings set --name $APP --resource-group $RG --settings SYS_ENVIRONMENT=PRODUCTION
```

### 4. Build, zip and deploy

```powershell
dotnet publish -c Release -o ../publish
Compress-Archive -Path ../publish/* -DestinationPath ../publish.zip -Force
az webapp deploy --name $APP --resource-group $RG --src-path ../publish.zip --type zip
```

### 5. Test

```powershell
curl.exe "https://$APP.azurewebsites.net/ServiceCheck"
curl.exe "https://$APP.azurewebsites.net/api/Weather"
```

Expected output:
```
The current environment is: PRODUCTION
The weather is snowy.
```

---

## Part 2 – Azure Function

The `CloudyWeather.Function` project is an isolated-worker (.NET 10) Function app with two anonymous HTTP-triggered functions, `ServiceCheck` and `Weather`, that mirror the API's controllers. It was created with:

```powershell
# from the cloudy-weather folder
func init CloudyWeather.Function --worker-runtime dotnet-isolated --target-framework net10.0
cd CloudyWeather.Function
func new --name ServiceCheck --template "HTTP trigger" --authlevel anonymous
func new --name Weather --template "HTTP trigger" --authlevel anonymous
cd ..
dotnet sln add CloudyWeather.Function
```

Run the commands below from the `CloudyWeather.Function` folder.

### 1. Variables

```powershell
$RG = "cloudy-weather-is"
$LOCATION = "norwayeast"
$STORAGE = "cloudyweatherbb6767"          # globally unique, lowercase letters/numbers only, 3–24 chars
$FUNCAPP = "cloudy-weather-func-bb6767"   # globally unique
```

If Part 1 was not run, create the resource group first:
```powershell
az group create --name $RG --location $LOCATION
```

### 2. Create storage account and Function App (Flex Consumption)

```powershell
az storage account create --name $STORAGE --resource-group $RG --location $LOCATION --sku Standard_LRS
az functionapp create --name $FUNCAPP --resource-group $RG --storage-account $STORAGE --flexconsumption-location $LOCATION --runtime dotnet-isolated --runtime-version 10
```

### 3. Set environment variable

```powershell
az functionapp config appsettings set --name $FUNCAPP --resource-group $RG --settings SYS_ENVIRONMENT=PRODUCTION
```

### 4. Publish

```powershell
func azure functionapp publish $FUNCAPP
```

### 5. Test

```powershell
curl.exe "https://$FUNCAPP.azurewebsites.net/api/ServiceCheck"
curl.exe "https://$FUNCAPP.azurewebsites.net/api/Weather"
```

Expected output:
```
The current environment is: PRODUCTION
The weather is snowy.
```

---

## Running locally

**API** – reads `SYS_ENVIRONMENT` from `.env.local`:
```powershell
cd CloudyWeather.API
dotnet run --launch-profile http
```
Endpoints: `http://localhost:5087/ServiceCheck`, `http://localhost:5087/api/Weather`

**Function** – add `"SYS_ENVIRONMENT": "LOCAL"` to `Values` in `local.settings.json` (not committed), then:
```powershell
cd CloudyWeather.Function
func start
```
Endpoints: `http://localhost:7071/api/ServiceCheck`, `http://localhost:7071/api/Weather`

## Troubleshooting

- **`RequestDisallowedByAzure`** – Azure for Students only allows certain regions. List the allowed ones with:
  ```powershell
  az policy assignment list --query "[].parameters.listOfAllowedLocations.value" -o tsv
  ```
- **`MissingSubscriptionRegistration`** – run the `az provider register` commands in *Setup*.
- **`func` not recognized after installing Core Tools** – the terminal has a stale PATH. Reload it in the current session (or sign out/in of Windows):
  ```powershell
  $env:Path = [Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [Environment]::GetEnvironmentVariable("Path","User")
  ```
- `az webapp/functionapp config appsettings set` shows `"value": null` – values are redacted in the output; verify with `az webapp config appsettings list --name $APP --resource-group $RG -o table`.
