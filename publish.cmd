dotnet publish -p:ExcludeApp_Data=true --runtime linux-x64 --configuration Release --output .\bin\publish\ --self-contained true

rd /S /Q ".\bin\publish\App_Data\"

scp -r ".\bin\publish\*" "jared@192.168.0.2:/var/www/radial.lucency.co/"
ssh jared@192.168.0.2 chown -R jared:web /var/www/radial.lucency.co/
ssh jared@192.168.0.2 chmod -R 775 /var/www/radial.lucency.co/
echo Publish completed.
pause