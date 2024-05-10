import base64
import os
import time

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
                # wait for packet to arrive
                time.sleep(0.25)

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
        # Remove steps if the server incorporates a dialogue agent
        elif data.startswith("Step"):
            try:
                step_number = int(data.split(' ')[1])
                model_info = ' '.join(data.split(' ')[2:])

                if model_info == 'Model 1':
                    if step_number == 1:
                        sock.SendData("Step 1: Grab the black 2x2 plate with wheel holders and place it on the table")
                    elif step_number == 2:
                        sock.SendData("Step 2: Attach a black wheel to the plate with the wheel holders")
                    elif step_number == 3:
                        sock.SendData("Step 3: Attach the other black wheel to the plate with the wheel holders")
                    elif step_number == 4:
                        sock.SendData("Step 4: Grab the second black 2x2 plate with wheel holders and place it on the "
                                      "table")
                    elif step_number == 5:
                        sock.SendData("Step 5: Attach a black wheel to the new plate with the wheel holders")
                    elif step_number == 6:
                        sock.SendData("Step 6: Attach the other black wheel to the new plate with the wheel holders")
                    elif step_number == 7:
                        sock.SendData("Step 7: Insert the 2x4 yellow plate on top of one of the black plates and "
                                      "leave a 1x2 space on the other black plate")
                    elif step_number == 8:
                        sock.SendData("Step 8: Attach the yellow corner plate on top of the black plate")
                    elif step_number == 9:
                        sock.SendData("Step 9: Attach a 1x2 yellow plate in the middle of the model")
                    elif step_number == 10:
                        sock.SendData("Step 10: Attach the 2x2 yellow plate by the 1x2 yellow plate")
                    elif step_number == 11:
                        sock.SendData("Step 11: Attach the black panel with rounded corners on top of the yellow plate")
                    elif step_number == 12:
                        sock.SendData("Step 12: Attach the black panel with rounded corners on the other side of the "
                                      "previous panel")
                    elif step_number == 13:
                        sock.SendData("Step 13: Attach the black panel corner on top of the yellow plate")
                    elif step_number == 14:
                        sock.SendData("Step 14: Insert the other black panel corner onto the yellow plate")
                    elif step_number == 15:
                        sock.SendData("Step 15: Add the yellow 2x4 arched car fender in front of the model")
                    elif step_number == 16:
                        sock.SendData("Step 16: Add a 1x2 yellow plate near the black panels")
                    elif step_number == 17:
                        sock.SendData("Step 17: Add a 1x2 guided yellow plate near the previously inserted plate")
                    elif step_number == 18:
                        sock.SendData("Step 18: Attach the 2x2 blue slope onto the yellow plate")
                    elif step_number == 19:
                        sock.SendData("Step 19: Attach the 2x2 yellow tile with a groove on top of the blue slope")

                    image_filename = os.path.join("Images", f"step{step_number}.jpg")
                    # Send image data over socket in chunks
                    send_image_over_socket_in_chunks(image_filename)

                if model_info == 'Model 2':
                    if step_number == 1:
                        sock.SendData("Step 1: Grab the black 2x2 plate with wheel holders and place it on the table")
                    elif step_number == 2:
                        sock.SendData("Step 2: Attach a black wheel rim to the plate with the wheel holders")
                    elif step_number == 3:
                        sock.SendData("Step 3: Attach the black wheel tire to the black wheel rim on the plate with "
                                      "the wheel holders")
                    elif step_number == 4:
                        sock.SendData("Step 4: Attach the other black wheel rim to the plate with the wheel holders")
                    elif step_number == 5:
                        sock.SendData("Step 5: Attach another black wheel tire to the black wheel rim on the plate "
                                      "with the wheel holders")
                    elif step_number == 6:
                        sock.SendData("Step 6: Grab the second black 2x2 plate with wheel holders and place it on the "
                                      "table")
                    elif step_number == 7:
                        sock.SendData("Step 7: Attach a black wheel rim to the new plate with the wheel holders")
                    elif step_number == 8:
                        sock.SendData("Step 8: Attach the black wheel tire to the black wheel rim on the plate with "
                                      "the wheel holders")
                    elif step_number == 9:
                        sock.SendData("Step 9: Attach the other black wheel rim to the plate with the wheel holders")
                    elif step_number == 10:
                        sock.SendData("Step 10: Attach another black wheel tire to the black wheel rim on the plate "
                                      "with the wheel holders")
                    elif step_number == 11:
                        sock.SendData("Step 11:  Insert the 2x4 yellow plate on top of the black plates and leave a "
                                      "1x2 space empty on both black plates")
                    elif step_number == 12:
                        sock.SendData("Step 12: Attach a 1x2 yellow plate in one of the empty spaces of the black "
                                      "plates")
                    elif step_number == 13:
                        sock.SendData("Step 13: Add a 1x2 guided yellow plate on top of the black plate")
                    elif step_number == 14:
                        sock.SendData("Step 14: Add the yellow 2x4 arched car fender in front of the model")
                    elif step_number == 15:
                        sock.SendData("Step 15: Attach the yellow corner plate on top of the black plate")
                    elif step_number == 16:
                        sock.SendData("Step 16: Add a 1x2 yellow plate near the yellow corner plate")
                    elif step_number == 17:
                        sock.SendData("Step 17: Attach the 2x2 yellow plate on top of the yellow corner plate and the "
                                      "1x2 yellow plate")
                    elif step_number == 18:
                        sock.SendData("Step 18: Attach the 2x2 blue slope onto the yellow plate")
                    elif step_number == 19:
                        sock.SendData("Step 19: Attach the 2x2 yellow tile with a groove on top of the blue slope")

                    image_filename = os.path.join("Images", f"step_{step_number}.jpg")
                    # Send image data over socket in chunks
                    send_image_over_socket_in_chunks(image_filename)

            except Exception as e:
                print(f"Error processing step data: {e}")
            print(data)
        # else:
        #     # print new received data
        #     print(data)
