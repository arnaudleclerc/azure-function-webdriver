FROM microsoft/dotnet:2.2-sdk AS installer-env

COPY . /src/dotnet-function-app
RUN cd /src/dotnet-function-app && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

# Install Chrome for Selenium
#RUN curl https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb -o /chrome.deb
#RUN dpkg -i /chrome.deb || apt-get install -yf
#RUN rm /chrome.deb


# Install chromedriver for Selenium
#RUN apt-get update && \
	#apt-get install -y unzip libglib2.0-0 && \
	#wget https://chromedriver.storage.googleapis.com/77.0.3865.40/chromedriver_linux64.zip -P ~/ && \
	#unzip ~/chromedriver_linux64.zip -d /home/site/wwwroot/bin

#RUN chmod +x /home/site/wwwroot/bin/chromedriver

FROM mcr.microsoft.com/azure-functions/dotnet:2.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
	
RUN apt-get update && apt-get install -y chromium-driver libglib2.0-0

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]