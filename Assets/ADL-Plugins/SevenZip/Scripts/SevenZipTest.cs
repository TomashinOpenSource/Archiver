using UnityEngine;
using System;
using System.Threading;
using System.IO;
using System.Collections;


public class SevenZipTest : MonoBehaviour{
#if (!UNITY_TVOS && !UNITY_WEBGL)  || UNITY_EDITOR

    //we use some integer to get error codes from the lzma library (look at lzma.cs for the meaning of these error codes)
    private int lzres = 0, lzres4 = 0;
    private int lzres2 = 0, lzres3 = 0;

    private bool pass1, pass2;

    //for counting the time taken to decompress the 7z file.
    private float t1, t;

    //the test file to download.
    private string myFile = "test.7z";

    //the adress from where we download our test file
    private string uri = "https://dl.dropbox.com/s/16v2ng25fnagiwg/";

    private string ppath;

    private string log ="";
	
	private bool compressionStarted, downloadDone;
	
	private ulong tsize;

	//reusable buffer for lzma alone buffer to buffer encoding/decoding
	private byte[] buff;

	//fixed size buffers, that don't get resized, to perform compression/decompression of buffers in them and avoid memory allocations.
	private byte[] fixedInBuffer = new byte[1024*256];
	private byte[] fixedOutBuffer = new byte[1024*256];

    Thread th = null;


    //A 1 item integer array to get the current extracted file of the 7z archive. Compare this to the total number of the files to get the progress %.
    private int[] fileProgress = new int[1];

    void Start(){

		ppath = Application.persistentDataPath;
		
		//we are setting the lzma.persitentDataPath so the get7zinfo, get7zSize, decode2Buffer functions can work on separate threads!
		lzma.persitentDataPath = Application.persistentDataPath;

		#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
			ppath=".";
		#endif

		// a reusable buffer to compress/decopmress data in/from buffers
		buff = new byte[0];

        Debug.Log(ppath);

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //download a 7z test file
        //StartCoroutine(Download7ZFile());
        if (!File.Exists(ppath + "/" + myFile)) StartCoroutine(Download7ZFile()); else downloadDone = true;

        //download an lzma alone format file to test buffer 2 buffer encoding/decoding functions
        StartCoroutine(buff2buffTest());
    }
	
	

    void Update(){
        if (Input.GetKeyDown(KeyCode.Escape)) { Application.Quit(); }
    }
	

    void OnGUI(){

        if (downloadDone == true) {
            GUI.Label(new Rect(50, 0, 350, 30), "package downloaded, ready to extract");
            GUI.Label(new Rect(50, 30, 450, 40), ppath);

            //when we call the decompress of 7z archives function, show a referenced integer that indicate the current file beeing extracted.
			if (th != null){
				GUI.Label(new Rect(Screen.width - 90, 10, 90, 50), fileProgress[0].ToString());
                GUI.Label(new Rect(Screen.width - 90, 30, 90, 50), lzma.getBytesWritten().ToString() + " : " +lzma.getBytesRead().ToString());
			}
            
            GUI.Label(new Rect(50, 70, 400, 30),  "decompress Buffer: " + pass1.ToString());

            GUI.TextArea(new Rect(50, 260, 640, 240), log);

            GUI.Label(new Rect(50, 100, 400, 30), "compress Buffer  : " + pass2.ToString());

			if (GUI.Button(new Rect(50, 140, 250, 50), "start 7z test")){
				//delete the known files that are extracted from the downloaded example z file
				//it is important to do this when you re-extract the same files  on some platforms.
				if (File.Exists(ppath + "/1.txt")) File.Delete(ppath + "/1.txt");
				if (File.Exists(ppath + "/2.txt")) File.Delete(ppath + "/2.txt");
				log = "";
				compressionStarted = true;
				//call the decompresion demo functions.
				DoDecompression();
			}

			//decompress file from buffer
			#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR) && !UNITY_EDITOR_WIN
				if (GUI.Button(new Rect(320, 140, 250, 50), "File Buffer test")) {
					doFileBufferTest();
				}
			#endif
        }

        if (compressionStarted){
            //if the return code is 1 then the decompression was succesful.
            GUI.Label(new Rect(50, 200, 250, 40), "7z return code: " + lzres.ToString());
            //time took to decompress
            GUI.Label(new Rect(50, 230, 250, 50), "time: " + t1.ToString());

            if (lzres2!=0) GUI.Label(new Rect(50, 510, 250, 30),"lzma encoded "+lzres2.ToString());
			if (lzres3!=0) GUI.Label(new Rect(50, 540, 250, 30),"lzma decoded "+lzres3.ToString());
			if(lzres4>0) GUI.Label(new Rect(50, 570, 250, 30),"decoded to buffer: ok");
        }

    }


	

	void DoDecompression(){
		
		// Decompress the 7z file

		t = Time.realtimeSinceStartup;

        // the referenced progress int will indicate the current index of file beeing decompressed. Use in a separate thread to show it realtime.

        // to get realtime byte level decompression progress (from a thread), there are 2 ways:
        //
        // 1. use lzma.get7zSize function to get the total uncompressed size of the files and compare against the bytes written in realtime, calling the lzma.getBytesWritten function.
        //
        // 2. use the lzma.getFileSize (or buffer length for FileBuffers) to get the file size and compare against the bytes read in realtime, calling the lzma.getBytesRead function.
        fileProgress[0] = 0;

		lzres = lzma.doDecompress7zip(ppath + "/" + myFile, ppath + "/", fileProgress, true, true);

       
       log += "Bytes Read: " + lzma.getBytesRead().ToString() + "  Bytes Written: " + lzma.getBytesWritten().ToString() + "\n\n";

       log += "Headers size: " + lzma.getHeadersSize(ppath + "/" + myFile).ToString() + "\n";// this function will reset the bytesRead and bytesWritten

        // If your 7z archive has multiple files and you call the lzma.doDecompress7zip function, you can call the lzip.sevenZcancel() function to cancel the operation.

        // get the uncompressed size of an entry.
        ulong sizeOfEntry = lzma.get7zSize(ppath + "/" + myFile, "1.txt");

        // Extract an entry and get its progress.
        log += "Extract entry: " + lzma.doDecompress7zip(ppath + "/" + myFile, null, false, false, "1.txt").ToString() + " progress: " + ((sizeOfEntry/lzma.getBytesWritten())*100f).ToString() +"% \n";

        //read file names and file sizes of the 7z archive, store them in the lzma.ninfo & lzma.sinfo ArrayLists and return the total uncompressed size of the included files.
        tsize = lzma.get7zInfo(ppath + "/" + myFile);

		log += ("Total Size: " + tsize + "      trueTotalFiles: " + lzma.trueTotalFiles) + "\n\n";
		
		//Look through the ninfo and info ArrayLists where the file names and sizes are stored.
		if(lzma.ninfo != null){
			for (int i = 0; i < lzma.ninfo.Count; i++){
				log += lzma.ninfo[i] + " - " + lzma.sinfo[i] + "\n";
				//Debug.Log(i.ToString()+" " +lzma.ninfo[i]+"|"+lzma.sinfo[i].ToString());
			}
		}
	
        log += "\n";

		//get size of a specific file. (if the file path is null it will look in the arraylists created by the get7zInfo function
		log += ("Uncompressed Size: "+lzma.get7zSize(ppath + "/" + myFile, "1.txt")) + "\n";

		//setup the lzma compression level. (see more at the function declaration at lzma.cs)
		//This function is not multiple threads safe so call it before starting multiple threads with lzma compress operations.
		lzma.setProps(9);

        //set encoding properties. lower dictionary compresses faster and consumes less ram!
        lzma.setProps(9, 1 << 16);


        //encode an archive to lzma alone format
        lzres2 = lzma.LzmaUtilEncode( ppath + "/1.txt", ppath + "/1.txt.lzma");

        //write out bytes read/written. If called from a thread you can get the progress of the encoding
        Debug.Log("bytes read: " + lzma.getBytesRead() + " / bytes written: " + lzma.getBytesWritten());

		//decode an archive from lzma alone format
		lzres3 = lzma.LzmaUtilDecode( ppath + "/1.txt.lzma", ppath + "/1BCD.txt");

        //write out bytes read/written. If called from a thread you can get the progress of the encoding
         Debug.Log("bytes read: " + lzma.getBytesRead() + " / bytes written: " + lzma.getBytesWritten());

 		//decode a specific file from a 7z archive to a byte buffer
		var buffer = lzma.decode2Buffer( ppath + "/" + myFile, "1.txt");
		
		if (buffer != null) {
			File.WriteAllBytes(ppath + "/1AAA.txt", buffer);
			if (buffer.Length > 0) { log += ("Decode2Buffer Size: " + buffer.Length) + "\n"; lzres4=1; } 
		}
        
        //you might want to call this function in another thread to not halt the main thread and to get the progress of the extracted files.
        //for example:
		th = new Thread(Decompress); th.Start(); // faster then coroutine

        //calculate the time it took to decompress the file
        t1 = Time.realtimeSinceStartup - t;
	}

	
    //call from separate thread. here you can get the progress of the extracted files through a referenced integer.
	void Decompress() {
		lzres = lzma.doDecompress7zip(ppath + "/"+myFile , ppath + "/", fileProgress, true,true);
        compressionStarted = true;
        
    }




	void doFileBufferTest() {
		//For iOS, Android, Linux and MacOSX the plugin can handle a byte buffer as a file. (in this case the www.bytes)
		//This way you can extract the file or parts of it without writing it to disk.
		//
       #if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE_LINUX) && !UNITY_EDITOR_WIN
	   if (File.Exists(ppath + "/" + myFile)) {
			byte[] www = File.ReadAllBytes(ppath + "/" + myFile);
			log="";

            lzres = lzma.doDecompress7zip(null, ppath + "/", true,true,null, www);
                log+=lzres.ToString()+"\n";
                log += "bytes read: " + lzma.getBytesRead().ToString() + "\n";
                log += "headers size: " + lzma.getHeadersSize(null, www).ToString() + "\n";

            fileProgress[0] = 0;
            lzres = lzma.doDecompress7zip(null, ppath + "/", fileProgress, false,true,null, www);
                log += lzres + "\n progress files: " + fileProgress[0].ToString() + "  progress bytes: " + lzma.getBytesWritten() + "\n";

		    tsize = lzma.get7zInfo(null, www);
		         log += "total size: " + tsize.ToString() + "  number of files: " + lzma.trueTotalFiles.ToString()+"\n";
				 for(int i=0 ; i<lzma.ninfo.Count; i++) log += lzma.ninfo[i] + " - " + lzma.sinfo[i] + "\n";

            tsize = lzma.get7zSize(null, null, www);
                log += "\ntotal size: " + tsize.ToString()+"\n";

		    var buffer = lzma.decode2Buffer( null, "2.txt", www);
		
		    if (buffer != null) {
                 log += "\ndec2buffer: ok"+ "\n";
			    File.WriteAllBytes(ppath + "/2AAA_FILEBUFFER.txt", buffer);
			    if (buffer.Length > 0) { log += ("FileBuffer_Decode2Buffer Length: " + buffer.Length) + "\n"; } 
		    }

            
		}
        #endif
	}

    IEnumerator Download7ZFile() {

        //make sure a previous 7z file having the same name with the one we want to download does not exist in the ppath folder
        if (File.Exists(ppath + "/" + myFile)) File.Delete(ppath + "/" + myFile);

        Debug.Log("starting download");

        //replace the link to the 7zip file with your own (although this will work also)
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(uri + myFile)) {
            #if UNITY_5 || UNITY_4
                yield return www.Send();
            #else
                yield return www.SendWebRequest();
            #endif

            if (www.error != null) {
                Debug.Log(www.error);
            } else {
                downloadDone = true;
                log = "";
                //write the downloaded 7zip file to the ppath directory so we can have access to it
                //depending on the Install Location you have set for your app, set the Write Access accordingly!
                File.WriteAllBytes(ppath + "/" + myFile, www.downloadHandler.data);

                Debug.Log("download done");
            }
        }

    }


    IEnumerator buff2buffTest() {
        //BUFFER TO BUFFER lzma alone compression/decompression EXAMPLE
        //
        //An example on how to decompress an lzma alone file downloaded through www without storing it to disk
        //using just the www.bytes buffer.
        //Download a file.
        using (UnityEngine.Networking.UnityWebRequest w = UnityEngine.Networking.UnityWebRequest.Get("https://dl.dropbox.com/s/3e6i0mri2v3xfdy/google.jpg.lzma")) {
            #if UNITY_5 || UNITY_4
                yield return w.Send();
            #else
                yield return w.SendWebRequest();
            #endif

		    if(w.error==null){
			    //we decompress the lzma file in the buff buffer.
			    if(lzma.decompressBuffer( w.downloadHandler.data, ref buff )==0){
                    pass1 = true;
                    //we write it to disk just to check that the decompression was ok
				    File.WriteAllBytes( ppath+"/google.jpg",buff);
			    }else{
				    log += ("Error decompressing www.bytes to buffer") + "\n"; pass1 = false; 
			    }
		    }else{ 
			    log += (w.error) + "\n"; 
		    }
        }

        yield return new WaitForSeconds(0.5f);

        //Example on how to compress a buffer.
        if (File.Exists(ppath+"/google.jpg")){
			byte[] bt = File.ReadAllBytes( ppath+"/google.jpg");

			//compress the data buffer into a compressed buffer
			if(lzma.compressBuffer( bt ,ref buff)){

                pass2=true;
                //write it to disk just for checking purposes
				File.WriteAllBytes( ppath+"/google.jpg.lzma",buff);
				//print info
				log += ("uncompressed size in lzma: "+BitConverter.ToUInt64(buff,5)) + "\n";
				log += ("lzma size: "+buff.Length) + "\n";
			} else {
                pass2=false;
				log += ("could not compress to buffer ...") + "\n";
			}

			//FIXED BUFFER FUNCTIONS:
			int compressedSize = lzma.compressBufferFixed(bt, ref fixedInBuffer);
			log += (" #-> Compress Fixed size Buffer: " + compressedSize) + "\n";

			if(compressedSize>0) {
				int decommpressedSize = lzma.decompressBufferFixed(fixedInBuffer, ref fixedOutBuffer);
				if(decommpressedSize > 0) log += (" #-> Decompress Fixed size Buffer: " + decommpressedSize) + "\n";
			}

			bt =null;  	
		}

	}
#else
        void OnGUI(){
            GUI.Label(new Rect(10,10,500,40),"Please run the WebGL/tvOS demo.");
        }
#endif
}

