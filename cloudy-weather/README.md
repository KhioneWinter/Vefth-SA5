location: norwayeast 


az group delete --name $RG --yes
$LOCATION = "norwayeast"
az group create --name $RG --location $LOCATION
az appservice plan create --name $PLAN --resource-group $RG --sku F1 --is-linux


az appservice plan create --name $PLAN --resource-group $RG --sku F1 --is-linux


az provider register --namespace Microsoft.Storage --wait
az provider register --namespace Microsoft.Insights --wait


az webapp create --name $APP --resource-group $RG --plan $PLAN --runtime "DOTNETCORE:10.0"

az webapp config appsettings set --name $APP --resource-group $RG --settings SYS_ENVIRONMENT=PRODUCTION

az webapp deploy --name $APP --resource-group $RG --src-path ../publish.zip --type zip

curl.exe "https://$APP.azurewebsites.net/ServiceCheck"
 - The current environment is: PRODUCTION
curl.exe "https://$APP.azurewebsites.net/api/Weather"
 - The weather is snowy.

