.PHONY: default

default:
	cd ../data/traces && zip -q ../../submission/submission.zip *.nbt
	shasum -a 256 submission.zip
	