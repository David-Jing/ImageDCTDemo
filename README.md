Project File Link:

	https://vault.sfu.ca/index.php/s/GUjbU595JcnjWfd

Builds Avaliable for Download:

	MacOS		(Validated on MacOS Catalina 10.15.6)
	Windows		(Validated on Windows 10 2004)

Unity Game Engine Version Used:

	2019.4.8f1	(https://unity3d.com/unity/qa/lts-releases)

External Libraries Used:	(Included in source file)

	Accord.Math
	StandaloneFileBrowser

Directory of Source Code:

	/Unity Project/Assets/Code

Note:

	- The builds can be ran directly, without any prerequisites.
	- Unity projects can be build for WebGL, tvOS, PS4, iOS, Xbox One, and Android.

Method Call Stack:

	- When user inputs a DCT size:
		DCTToggles.cs:
			1. GetSizeInput()
			2. UpdateGridCount()
			3. GetDCTMatrix()
			4. LoadDCTComponentSprite()
			5. ToggleOn()
		ImageRenderer.cs:
			6. DCTCompUpdateCheck()
			7. UpdateDCTImage()
		DCTToggle.cs:
			8. GetDCTMatrix()

	- When user clicks on the Toggle Off / Toggle On button:
		DCTToggles.cs:
			1. ToggleOff() / ToggleOn()
		ImageRenderer.cs:
			2. DCTCompUpdateCheck()
			3. UpdateDCTImage()
		DCTToggle.cs
			4. GetDCTMatrix()

	- When user toggles a DCT component:
		ImageRenderer.cs:
			1. DCTCompUpdateCheck()
			2. UpdateDCTImage()
		DCTToggle.cs:
			3. GetDCTMatrix()

	- When user clicks on the Custom Image / Default Image button:
		FileDialog.cs:
			1. OpenExplorer() / RestoreDefault()
		ImageRenderer.cs:
			2. DCTCompUpdateCheck()
			3. UpdateDCTImage()
		DCTToggle.cs:
			4. GetDCTMatrix()

