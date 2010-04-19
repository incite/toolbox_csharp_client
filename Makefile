InciteSupportAPI.exe: InciteSupportAPI.cs
	gmcs -r:Newtonsoft.Json.dll InciteSupportAPI.cs
clean:
	rm inciteSupportAPI.exe
test:
	mono InciteSupportAPI.exe 1
