from azure.servicebus.aio import ServiceBusClient
from azure.servicebus import ServiceBusMessage
from config import settings
import json
import logging
from typing import Dict, Any

logger = logging.getLogger(__name__)

class ServiceBusService:
    def __init__(self):
        self.connection_string = settings.azure_service_bus_connection_string
        self.queue_name = settings.azure_service_bus_queue_name
    
    async def send_cv_processing_message(self, message_data: Dict[str, Any]) -> bool:
        """Send CV processing message to Service Bus queue"""
        try:
            async with ServiceBusClient.from_connection_string(self.connection_string) as client:
                sender = client.get_queue_sender(queue_name=self.queue_name)
                async with sender:
                    message = ServiceBusMessage(json.dumps(message_data))
                    await sender.send_messages(message)
                    logger.info(f"Message sent to queue: {message_data['fileId']}")
                    return True
        except Exception as e:
            logger.error(f"Error sending message to queue: {e}")
            return False

service_bus = ServiceBusService()
