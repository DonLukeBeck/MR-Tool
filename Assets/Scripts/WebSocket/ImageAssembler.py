import base64
import tempfile
from io import BytesIO
from PIL import Image


class ImageAssembler:
    def __init__(self):
        self.image_chunks = []
        self.total_chunks = 0
        self.received_chunks = 0

    def process_image_data(self, image_data):
        # Parse metadata
        if image_data.startswith("ImageChunks"):
            self.total_chunks = int(image_data.split(' ')[1])
            # print(f"Total chunks: {self.total_chunks}")
            return
        # Receive image data chunks
        if image_data.startswith("Base64EncodedChunk"):
            # Extract Base64 encoded image data
            base64_data = image_data.split(' ')[1]
            # print(f"Received chunk {self.received_chunks + 1}/{self.total_chunks}")
            # print(base64_data)
            # Ensure length is valid for decoding
            if len(base64_data) % 4 != 0:
                print(len(base64_data))
                # Add padding if necessary to make length a multiple of 4
                base64_data += '=' * (4 - len(base64_data) % 4)
            try:
                # Decode Base64 encoded image data to bytes
                chunk_data = base64.b64decode(base64_data)
                self.image_chunks.append(chunk_data)
                self.received_chunks += 1
            except Exception as e:
                print(f"Error decoding Base64 data: {e}")

        # If all chunks received, assemble image
        if self.received_chunks == self.total_chunks:
            self.assemble_image()
            self.received_chunks = 0
            self.total_chunks = 0

    def assemble_image(self):
        # Concatenate chunks
        image_data = b''.join(self.image_chunks)
        self.image_chunks.clear()
        try:
            # Reconstruct image from byte array
            reconstructed_image = Image.open(BytesIO(image_data))
            print("Image reconstructed")
            # Use tempfile to create a temporary file
            with tempfile.NamedTemporaryFile(suffix=".jpg", delete=False) as temp_file:
                temp_file_path = temp_file.name
                reconstructed_image.save(temp_file_path)

            # This is the image that needs to be used by the dialogue agent to determine the current step
            # Display the temporary image file - this is just for testing purposes
            reconstructed_image.show()
        except Exception as e:
            print(f"Error reconstructing image: {e}")
