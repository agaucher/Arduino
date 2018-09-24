#include <SPI.h>
#include <Ethernet2.h>

const int PIN = 6; 
const unsigned int MAX_DATA_LENGTH = 300;
byte mac[] = {
  0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xED
};
const int PORT = 9800;
EthernetServer server(PORT);

void setup() {
  Serial.begin(9600);
  Serial.println("Initialization");

  pinMode(PIN, OUTPUT);

  while (Ethernet.begin(mac) == 0)
  {
    Serial.println ("Cannot get an IP address using DHCP protocol. Error !");
    delay (10000);
  }

  Serial.println("Server web is started");
  Serial.print("Listening on following interface: ");
  Serial.print(Ethernet.localIP());
  Serial.print(" (");
  Serial.print(PORT);
  Serial.println(")");
}

void loop() {
  EthernetClient client = server.available();
  if (client) {
    
    Serial.println("-----------------------------");
    Serial.println("Receiving new http request...\n");
    
    char data[MAX_DATA_LENGTH];
    int index = 0;

    boolean currentLineIsBlank = true;
    while (client.connected()) {
      if (client.available()) {
        char c = client.read();

        if (index < MAX_DATA_LENGTH-1)
        {
          data[index] = c;
          index ++;
        }
        
        if (c == '\n' && currentLineIsBlank) {
          data[index] = '\0';
          Serial.println(data);

          if (strstr(data, "GET /openDoor HTTP"))
          {
            handleOpenDoorRequest(&client, data);
          }
          else
          {
            // URL is wrong ! 
            sendResponse(&client, 404, (char*)"");
          }

          break;
        }
        if (c == '\n') {
          currentLineIsBlank = true;
        }
        else if (c != '\r') {
          currentLineIsBlank = false;
        }
      }
    }
    client.stop();
    Serial.println("client disconnected");
    Serial.println("-----------------------------");
  }
  delay (50);
}

void handleOpenDoorRequest(EthernetClient* client, char* data)
{
  if (isUserAuthorized(data))
  {
    openDoor();
    sendResponse(client, 200, (char*)"Done!");
  }
  else
  {
    sendResponse(client, 403, (char*)"");  
  }
}

bool isUserAuthorized(char* data)
{
  bool success = false;

  success |= strstr(data, "Authorization: Basic YWdhdWNoZXI6OWghTDIqUXA=") != NULL;
  success |= strstr(data, "Authorization: Basic ZWxlc3RyYkQ6NE0zeQosciE=") != NULL;
  
  return success;
}

void openDoor()
{
  Serial.println("opening door");
  digitalWrite(PIN, HIGH);
  delay(1250);
  digitalWrite(PIN, LOW);
}

void sendResponse(EthernetClient* client, int code, char* body)
{
  char response[MAX_DATA_LENGTH];

  Serial.println("Reply to client: ");
  
  switch(code)
  {
    case 200:
      strcpy(response, "HTTP/1.1 200 OK\n");
      break;
      
    case 404:
      strcpy(response, "HTTP/1.1 404 Not Found\n");
      break;

    case 403:
      strcpy(response, "HTTP/1.1 403 Forbidden\n");
      break;
    
    default:
      strcpy(response, "HTTP/1.1 500 Internal Server Error\n");
  }

   strcat(response, "Content-Type: text/html\n");
   strcat(response, "Connection: close\n");
   strcat(response, "\n");

   strcat(response, body);
   strcat(response, "\n");

   Serial.println(response);
   client->print (response);
}

