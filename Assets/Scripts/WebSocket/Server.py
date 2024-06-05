import base64
import os
from time import sleep

import UdpComms as U
from Assets.Scripts.WebSocket.ImageAssembler import ImageAssembler

# Initialize the ImageAssembler that handles receiving and reconstructing images
assembler = ImageAssembler()

# Create UDP socket to use for sending (and receiving)
sock = U.UdpComms(udpIP="127.0.0.1", portTX=8000, portRX=8001, enableRX=True, suppressWarnings=True)

# Data to be used by the dialogue agent to determine the current step
# Remove this if dialogue agent can determine the step on its own
model_info = ''
step_number = ''


# Function to send image data over socket in chunks
def send_image_over_socket_in_chunks(image_name, chunk_size=8192):
    with open(image_name, "rb") as image_file:
        # Read image data
        image_data = image_file.read()
        # Calculate total number of chunks
        total_chunks = (len(image_data) + chunk_size - 1) // chunk_size

        sleep(0.5)
        # Send total number of chunks to client
        sock.SendData(f"ImageChunks {total_chunks}")

        # Send image data in chunks
        for i in range(total_chunks):
            # wait for packet to arrive
            sleep(0.5)

            offset = i * chunk_size
            chunk = image_data[offset:offset + chunk_size]
            # Encode image chunk as Base64
            chunk_base64 = base64.b64encode(chunk).decode('utf-8')
            # Send image chunk
            sock.SendData(f"Base64EncodedChunk {chunk_base64}")


# Function to handle received questions
def handle_question(question):
    # Wizard of Oz approach to replace the dialogue agent until an AI model is connected
    print(question)
    user_input = input("Answer: ")
    sleep(0.5)
    options = input("Send (1) User Input, (2) Default Message, (3) Both: ")

    default_message = ("I am unable to determine your current step, please provide a picture of the model by looking "
                       "towards it and pressing the \"Send Picture\" button.")

    if options == '1':
        response = user_input
    elif options == '2':
        response = default_message
    elif options == '3':
        response = user_input + " " + default_message
    else:
        # invalid option,sending default message
        response = default_message

    sleep(0.5)
    sock.SendData("Answer: " + response)


# Function to handle step instructions
def handle_step(data):
    global model_info
    model_info = ' '.join(data.split(' ')[2:])
    if model_info == 'Model 1':
        instructions = {
            1: "Step 1: Grab the black 2x2 plate with wheel holders and place it on the table",
            2: "Step 2: Attach a black wheel to the plate with the wheel holders",
            3: "Step 3: Attach the other black wheel to the plate with the wheel holders",
            4: "Step 4: Grab the second black 2x2 plate with wheel holders and place it on the table",
            5: "Step 5: Attach a black wheel to the new plate with the wheel holders",
            6: "Step 6: Attach the other black wheel to the new plate with the wheel holders",
            7: "Step 7: Insert the 2x4 yellow plate on top of one of the black plates and leave a 1x2 space on the "
               "other black plate",
            8: "Step 8: Attach the yellow corner plate on top of the black plate",
            9: "Step 9: Attach a 1x2 yellow plate in the middle of the model",
            10: "Step 10: Attach the 2x2 yellow plate by the 1x2 yellow plate",
            11: "Step 11: Attach the black panel with rounded corners on top of the yellow plate",
            12: "Step 12: Attach the black panel with rounded corners on the other side of the previous panel",
            13: "Step 13: Attach the black panel corner on top of the yellow plate",
            14: "Step 14: Insert the other black panel corner onto the yellow plate",
            15: "Step 15: Add the yellow 2x4 arched car fender in front of the model",
            16: "Step 16: Add a 1x2 yellow plate near the black panels",
            17: "Step 17: Add a 1x2 guided yellow plate near the previously inserted plate",
            18: "Step 18: Attach the 2x2 blue slope onto the yellow plate",
            19: "Step 19: Attach the 2x2 yellow tile with a groove on top of the blue slope"
        }
    elif model_info == 'Model 2':
        instructions = {
            1: "Step 1: Grab the black 2x2 plate with wheel holders and place it on the table",
            2: "Step 2: Attach a black wheel rim to the plate with the wheel holders",
            3: "Step 3: Attach the black wheel tire to the black wheel rim on the plate with the wheel holders",
            4: "Step 4: Attach the other black wheel rim to the plate with the wheel holders",
            5: "Step 5: Attach another black wheel tire to the black wheel rim on the plate with the wheel holders",
            6: "Step 6: Grab the second black 2x2 plate with wheel holders and place it on the table",
            7: "Step 7: Attach a black wheel rim to the new plate with the wheel holders",
            8: "Step 8: Attach the black wheel tire to the black wheel rim on the plate with the wheel holders",
            9: "Step 9: Attach the other black wheel rim to the plate with the wheel holders",
            10: "Step 10: Attach another black wheel tire to the black wheel rim on the plate with the wheel holders",
            11: "Step 11: Insert the 2x4 yellow plate on top of the black plates and leave a 1x2 space empty on both "
                "black plates",
            12: "Step 12: Attach a 1x2 yellow plate in one of the empty spaces of the black plates",
            13: "Step 13: Add a 1x2 guided yellow plate on top of the black plate",
            14: "Step 14: Add the yellow 2x4 arched car fender on the other side of the model",
            15: "Step 15: Attach the yellow corner plate on top of the arched car fender",
            16: "Step 16: Add a 1x2 yellow plate behind the yellow corner plate",
            17: "Step 17: Attach the 2x2 yellow plate on top of the yellow corner plate and the 1x2 yellow plate",
            18: "Step 18: Attach the 2x2 blue slope onto the yellow plate",
            19: "Step 19: Attach the 2x2 yellow tile with a groove on top of the blue slope"
        }
    global step_number
    step_number = int(input("Enter step number to send: "))

    image_filename = os.path.join("Images",
                                  f"step{step_number}.jpg" if model_info == 'Model 1' else f"step_{step_number}.jpg")

    sleep(0.5)
    send_image_over_socket_in_chunks(image_filename)

    sleep(0.5)
    sock.SendData(instructions[int(step_number)])


while True:
    data = sock.ReadReceivedData()  # read data
    if data is not None:  # if NEW data has been received since last ReadReceivedData function call
        # Check if received data is an image chunk
        if data.startswith("ImageChunks") or data.startswith("Base64EncodedChunk"):
            assembler.process_image_data(data)
        # Check if received data is a question
        elif data.startswith("Question"):
            handle_question(data)
        # Handle step data
        elif data.startswith("Step"):
            # print(data)
            handle_step(data)
        elif data.startswith("Resend Image"):
            # print(data)
            image_filename = os.path.join("Images",
                                          f"step{step_number}.jpg" if model_info == 'Model 1' else f"step_{step_number}.jpg")

            sleep(0.5)
            send_image_over_socket_in_chunks(image_filename)
        else:
            # print new received data
            print(data)
