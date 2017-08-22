The existing code uses a string as a layername, the code in this folder
uses a LayerName object to organize the dataframe and group names
instead of embeding path separators in the layername string.
The LayerName object is better, 
but it requires a new COM type to be marshalled between COM/.Net
whereas an array of strings will be marshalled automatically.
Due to my concern with marshalling issues, this code has not been
rolled into the trunk.