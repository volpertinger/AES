# AES Algorithm file encryption and decryption

## Settings

### Key [String] - 16 (128 bit) / 24 (192 bit) / 32 (256 bit) symbols string

### KeyLength - length of the key used

### BlockChainMode - encryption mode

#### Possible values

* ECB - Electronic Codebook

* CBC - Cipher Block Chaining

* OFB - Output Feedback

* CFB - Cipher Feedback

### BatchSize - Number of blocks to read, write to a file during one iteration of encryption or decryption

### Operations [array[Operation]] - array of encryption and decryption operations

#### Operation

* PathInput [string] - input file to perform the operation

* PathOutput [string] - output path to save the result (if the file already exists, the operation will be aborted)

* Operation [string] - two values are possible: Encrypt and Decrypt

## Usage

* Fill settings correctly

* Launch program