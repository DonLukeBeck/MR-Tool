import base64
import os

import UdpComms as U
from Assets.Scripts.WebSocket.ImageAssembler import ImageAssembler

# Initialize the ImageAssembler
assembler = ImageAssembler()

# Create UDP socket to use for sending (and receiving)
sock = U.UdpComms(udpIP="127.0.0.1", portTX=8000, portRX=8001, enableRX=True, suppressWarnings=True)


# Function to send image data over socket in chunks
def send_image_over_socket_in_chunks(image_name, chunk_size=8192):
    try:
        with open(image_name, "rb") as image_file:
            # Read image data
            image_data = image_file.read()
            # Calculate total number of chunks
            total_chunks = (len(image_data) + chunk_size - 1) // chunk_size
            # Send total number of chunks to server
            sock.SendData(f"ImageChunks {total_chunks}")

            # Send image data in chunks
            for i in range(total_chunks):
                offset = i * chunk_size
                chunk = image_data[offset:offset + chunk_size]
                # Encode image chunk as Base64
                chunk_base64 = base64.b64encode(chunk).decode('utf-8')
                # Send image chunk
                sock.SendData(f"Base64EncodedChunk {chunk_base64}")
    except FileNotFoundError:
        print(f"Image file {image_name} not found.")
    except Exception as e:
        print(f"Error sending image data: {e}")


while True:

    data = sock.ReadReceivedData()  # read data
    if data is not None:  # if NEW data has been received since last ReadReceivedData function call
        # Check if received data is an image chunk
        if data.startswith("ImageChunks") or data.startswith("Base64EncodedChunk"):
            assembler.process_image_data(data)
        # Check if received data is a question
        elif data.startswith("Question"):
            sock.SendData("I am unable to determine your current step, please provide a picture of the model by "
                          "looking towards it and pressing the \"Send Picture\" button")  # send response
            print(data)
        elif data.startswith("Step"):
            try:
                step_number = int(data.split(' ')[1])
                image_filename = os.path.join("Images", f"step{step_number}.jpg")
                print(image_filename)
                # Send image data over socket in chunks
                send_image_over_socket_in_chunks(image_filename)
            except Exception as e:
                print(f"Error processing step data: {e}")
        # else:
        #     # print new received data
        #     print(data)
