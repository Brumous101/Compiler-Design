
#This should have the command needed to run your compiler executable
#I'm using the Mono toolchain -- hence the 'mono' prefix
#You probably won't need that, but you should put the full path here.
#Ex: COMPILER = [ r"C:\users\bob\etec4401\lab\bin\compiler.exe" ]
#COMPILER=["mono", "bin/Debug/parser.exe"]
COMPILER=["Compiler.exe"]

#restrict tests to run
#First = group number. None=all
#Second = first test to run. Can be None (all tests in the group)
#or a number.
#Third = last test to run. None = last test in group.
#runOnly = (None,0,None)   #run all tests
runOnly = (0,0,None)     #run all tests from group 1
#runOnly = (0,3,3)        #run group 0, test 3
#runOnly = (0,4,10)      #run group 0, tests 4-10
#runOnly = (0,4,None)      #run group 0, tests 4, 5, 6, ...








import subprocess
import sys
import os
import os.path
import json
import re




#set this to true to get more status messages
VERBOSE=False

#how long to wait for subprocesses, in seconds
TIMEOUT = 2

#these are the input and output files
inputfile = "input.txt"
asmfile = "out.asm"
objfile = "out.o"
exefile = "out.exe"

#newline symbol
NEWLINE = "\u23ce";


#VS2019
#need to install Build Tools for VS 2019 in the VS installer.
#https://stackoverflow.com/questions/55097222/vcvarsall-bat-for-visual-studio-2019
VSBAT=r"c:\program files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat"

DELIMITER="_-_-_-_-DELIMITER-_-_-_-_"
subshell = None

if sys.platform == "win32":
    if not os.access(VSBAT,os.F_OK):
        print("The VS2019 tools are not installed. I can't continue.")
        print("See: https://stackoverflow.com/questions/55097222/vcvarsall-bat-for-visual-studio-2019")
        sys.exit(1)


#Input: Assembly code.
#Returns: Object file
def assemble( asmfile, objfile ):
    if sys.platform == "linux":
        args = ["-gdwarf","-f","elf64"]
    elif sys.platform == "win32":
        args = ["-g","-f","win64"]
    elif sys.platform == "darwin":
        args = ["-g","--prefix","_","-f","macho64"]
    else:
        raise RuntimeError("Unknown platform for assemble: "+sys.platform)
        
    args.append("-Werror");
    args.append("-o");
    args.append(objfile);
    args.append(asmfile);
    cmd = ["nasm"] + args
    if VERBOSE:
        print(cmd)
    try:
        subprocess.check_call( cmd )
    except:
        print("Could not execute command:",cmd)
        raise

#assemble asmfile to objfile
#Call callback when done.
#callback gets one argument
#   None if no error
#   an Exception otherwise
# ~ def assembleAsync(asmfile,objfile,callback):
    # ~ try:
        # ~ assemble(asmfile,objfile)
        # ~ callback(None)
    # ~ except Exception as e:
        # ~ callback(e)

def access(fname):
    return os.access(fname,os.F_OK)
     

#Return tuple for shell that can be used to run the compiler
#   ( name of shell , commands that should be executed )
def getShell():
    if sys.platform == "linux":
        return ("/bin/sh","")
    elif sys.platform == "win32":
        return ("cmd", '"{}"\ncd {}\necho off\n'.format( VSBAT, os.getcwd() ) )
    elif sys.platform == "darwin":
        return ("/bin/sh","")
    else:
        raise RuntimeError("Unknown platform")


#get shell command to set environment variables
def getSet():
    if sys.platform == "linux":
        return "export"
    elif sys.platform == "win32":
        return "set"
    elif sys.platform == "darwin":
        return "export"
    else:
        raise RuntimeError("Unknown platform")

#get command to run linker
#returns  tuple:
#   executable, ( tuple of arguments ) )
def getLinker( objfile, exefile):
    if sys.platform == "linux":
        return ( "gcc", ("-m64",objfile,"-o",exefile) )
    elif sys.platform == "win32":
        #for vs2019:
        #  oldnames.lib has fdopen
        #  msvcrt.lib has mainCRTStartup and fflush
        #  legacy_stdio_definitions.lib has fprintf
        return ( "link", (objfile,"/OUT:"+exefile,
                    "/SUBSYSTEM:CONSOLE", "/LARGEADDRESSAWARE:no",
                    "/nologo", "msvcrt.lib", "oldnames.lib",
                    "legacy_stdio_definitions.lib") )
    elif sys.platform == "darwin":
        return ("ld", ("-o", exefile, objfile, "-macosx_version_min", "10.13", "-lSystem"))
    else:
        raise RuntimeError("Unknown platform")

#get a command to print the last process's exit code
def getExitCode(pfx):
    if sys.platform == "linux" or sys.platform == "darwin":
        return "echo {} $?".format(pfx)
    elif sys.platform == "win32":
        return "echo {} %ErrorLevel%".format(pfx)
    else:
        raise RuntimeError("Unknown platform")
    
#wait until the subshell outputs the delimiter
#return all printed text
def waitForDelimiter():
    if VERBOSE:
        print("Waiting for delimiter...")
        
    lines=[]
    line=""
    while True:
        c = subshell.stdout.read(1)
        if len(c) == 0:
            raise RuntimeError("Shell exited unexpectedly")
            
        c=c.decode()
        if VERBOSE:
            sys.stdout.write(c)
            sys.stdout.flush()
            
        if c == "\r":
            pass
        else:
            line += c
            if c == "\n":
                lines.append(line)
                line=""
                if lines[-1].strip() == DELIMITER: # in lines[-1]:
                    return "\n".join(lines)
                    
            
    
#do the link using the subshell
def link(objfile,exefile):

    unlink(exefile)

    startSubshell()

    ldexe,ldargs = getLinker(objfile,exefile)
    cmd = "{} {}\n{}\necho {}\n".format(
        ldexe, 
        " ".join(ldargs), 
        getExitCode("xLinkx"), 
        DELIMITER )
    if VERBOSE:
        print(cmd)
    subshell.stdin.write( cmd.encode() )
    subshell.stdin.flush()
    txt = waitForDelimiter()
    txt = txt.replace("\r","")
    tmp = txt.strip().split("\n")
    tmp = list( filter(lambda x: len(x.strip()) > 0, tmp ) )
    
    if VERBOSE:
        print(tmp)
    if tmp[-1].strip() != DELIMITER:
        print("Expected delimiter, got:")
        print(tmp[-1])        
        assert 0
#    assert tmp[-1].strip() == DELIMITER
    status = tmp[-2].strip()
    if not status.startswith("xLinkx"):
        status = tmp[-3]
    if not status.startswith("xLinkx"):
        print("Unexpected status:",status)
        assert 0
#    assert status.startswith("xLinkx")
    errcode = status.split()[1]
    #don't look at error code; just see if exe was created
    if not os.access(exefile,os.F_OK):
        raise RuntimeError("Link failed");

#start a subshell. If one has already been started, do nothing.
def startSubshell():
    global subshell
    
    if subshell != None:
        return
    
    if VERBOSE:    
        print("Starting worker shell...")
    
    exe,inp = getShell()
    subshell = subprocess.Popen( [exe],
        stdin=subprocess.PIPE, stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT)
    
    subshell.stdin.write(inp.encode())
    subshell.stdin.write(b"\n")
    subshell.stdin.flush()
    
    if VERBOSE:
        print("Worker shell up")

def quit():
    if subshell:
        subshell.stdin.write("exit\nexit\n".encode())
        subshell.stdin.flush()
        subshell.wait()
         


class ReturnStatus:
    def __init__(self,msg):
        self.msg=msg
    def __str__(self):
        return self.msg

NEVER_RETURN = ReturnStatus("No return")
DONT_CARE = ReturnStatus("Don't care")

 
#like the Unix (tm) nl utility: Number Lines
#x=string
def nl(x):
    x=x.replace("\r\n","\n")
    for i,line in enumerate(x.split("\n")):
        txt = line.rstrip()
        print("{:4d} | {}".format(i+1,txt))


# ~ function codepointToString(codepoint: number){
    # ~ //http://crocodillon.com/blog/parsing-emoji-unicode-in-javascript
    # ~ let offset = codepoint - 0x10000;
    # ~ let offsethi = (offset>>10)+0xd800;
    # ~ let offsetlo = 0xdc00 + (offset & 0x3ff);
    # ~ let sepchar = String.fromCharCode(offsethi) + String.fromCharCode(offsetlo)+" ";
    # ~ //variant selector: monochrome: https://stackoverflow.com/questions/32915485/how-to-prevent-unicode-characters-from-rendering-as-emoji-in-html-from-javascrip
    # ~ //sepchar += "\ufe0e";
    # ~ return sepchar;
# ~ }

SEP="-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-="

if sys.platform == "linux":
    badEmoji  = "\U0001f630"  # 0x1f644 0x1f9d0 0x1f92a 
    goodEmoji = "\U0001f601"
else:
    badEmoji = ":-("
    goodEmoji = ":-)"

#list of numbers: group i has numCases[i] test cases
numCases = []

#group i had numOK[i] tests pass
numOK = []

#group i had numBad[i] tests fail
numBad = []

#if group i cannot continue, groupsThatCannotContinue[i] is true
groupsThatCannotContinue = []

#list of test cases, sorted by bonus number
#within a bonus number, cases appear in the order
#they were encountered in the input file
workqueue = []

#called when a test passes
def good(groupNumber, testNumber):
    numOK[groupNumber] += 1
    if groupNumber == 0:
        txt = "Test"
    else:
        txt = "Bonus"
    if VERBOSE:
        print(txt,"group",groupNumber,"/ Test",testNumber,"OK")

#called when a test fails
def bad(groupNumber, testNumber):
    numBad[groupNumber] += 1
    groupsThatCannotContinue[groupNumber]=True;
    if groupNumber == 0:
        txt = "Test"
    else:
        txt = "Bonus"
    if VERBOSE:
        print(txt,"group",groupNumber,"/ Test",testNumber,"failed")

#called if we skip a test
def skip(groupNumber,testNumber, quiet):
    if not quiet:
        print(testNumber,": Test skipped");

def printSummary():
    for i in range(len(numCases)):
        if i == 0 :
            s1="Total non-bonus tests:     "
            s2="Total non-bonus tests OK:  "
            s3="Total non-bonus tests bad: "
        else:
            s1="Bonus group "+str(i)+": Tests:      "
            s2="Bonus group "+str(i)+": OK:         "
            s3="Bonus group "+str(i)+": Bad:        "

        if numOK[i] == numCases[i]:
            emoji = goodEmoji
        else:
            emoji = badEmoji
            
        print(s1,numCases[i])
        print(s2,numOK[i],"   ",emoji)
        print(s3,numBad[i],"\n")

def unlink(fname):
    try:
        os.unlink(fname)
    except OSError:
        pass
    if os.access(fname,os.F_OK):
        raise RuntimeError("Could not delete "+fname)
        
def runOneTestcase( groupNumber, testNumber, testcase ):

    if groupsThatCannotContinue[ groupNumber ] :
        skip(groupNumber, testNumber, True )
        return
    
    print(SEP);
    print("Group {}: Test {} of {}".format(
        groupNumber, testNumber, numCases[groupNumber] ) )

    unlink(inputfile)
    unlink(asmfile)
    unlink(objfile)
    unlink(exefile)
    
    input = testcase.get("code")
    syntaxOK = testcase.get("syntaxOK",True)            #bool
    expectedReturn = testcase.get("returns",DONT_CARE)  #int
    if expectedReturn == None:
        expectedReturn = NEVER_RETURN
    expectedStdout = testcase.get("output",DONT_CARE)    #string
    if expectedStdout == None:
        expectedStdout = DONT_CARE
    outputfiles = testcase.get("outputfiles",[])  #list of pair: (string,string): name, contents
    stdin = testcase.get("stdin","")              #string
        
        
    if not syntaxOK and expectedReturn is not NEVER_RETURN and expectedReturn is not DONT_CARE:
        print(input)
        raise RuntimeError("syntaxOK==false but expectedReturn acts like it returns")
        
    def printStatus(abbreviated):
        print("------------------------------")
        print("Input:")
        nl(input)
        print("------------------------------")
        print("Assembly code:")
        try:
            with open(asmfile) as fp:
                asmdata = fp.read()
            nl(asmdata);
        except FileNotFoundError:
            print("(no assembly output file)")

        print("------------------------------")

        if abbreviated:
            return
            
        print("Actual return code:  ",actualReturn)
        print("Expected return code:",expectedReturn)
        print("------------------------------");
        if expectedStdout != DONT_CARE:
            exp = expectedStdout.replace("\n",NEWLINE)
        else:
            exp = "<don't care>";
        print("Expected stdout: "+exp);
        print("Actual stdout:   "+actualStdout.replace("\n",NEWLINE))
        print("------------------------------")
        for fname,expectedcontents in outputfiles:
            print("Output file: ",fname)
            print("Expected contents: "+expectedcontents.replace("\n",NEWLINE))
            if not os.access(fname,os.F_OK) :
                print("Actual contents:   <missing>")
            else:
                with open(fname,errors="replace") as fp:
                    actualcontents = fp.read()
                print("Actual contents:   "+actualcontents.replace("\n",NEWLINE))
            print("------------------------------")
    
      
    with open(inputfile,"w") as fp:
        fp.write(input)
        
    cmd = COMPILER + [inputfile]
    try:
        subprocess.check_call(cmd)
        accepted=True
    except subprocess.CalledProcessError:
        accepted=False
    except RuntimeError:
        accepted=False
        
    if accepted and syntaxOK:
        pass
    elif accepted and not syntaxOK:
        print("Accepted invalid input");
        printStatus(True)
        bad(groupNumber,testNumber)
        return False
    elif not accepted and not syntaxOK:
        #we expected it not to compile
        good(groupNumber,testNumber)
        return True
    elif not accepted and syntaxOK:
        #this should have compiled
        print("Rejected valid input")
        printStatus(True)
        bad(groupNumber,testNumber)
        return False
    else:
        assert 0
        
    try:
        assemble(asmfile,objfile)
    except subprocess.CalledProcessError:
        print("Error assembling file")
        printStatus(True)
        bad(groupNumber,testNumber)
        return False
            
    try:
        link( objfile, exefile)
    except RuntimeError:
        print("Error linking file")
        printStatus(True)
        bad(groupNumber,testNumber)
        return false
        
    
    P = subprocess.Popen( [os.path.join(".",exefile)],
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE )
    
    try:
        actualStdout,_ = P.communicate( stdin, timeout=TIMEOUT )
        actualStdout = actualStdout.decode()
        actualReturn = P.returncode
        if actualReturn < 0:
            print("Process crashed")
    except subprocess.TimeoutExpired:
        P.terminate()
        P.wait()
        actualReturn = NEVER_RETURN
        actualStdout = ""
        

    if  expectedReturn is not DONT_CARE and actualReturn != expectedReturn:
        print("Return value mismatch")
        printStatus(False)
        bad(groupNumber,testNumber)
        return False
        
           
    if expectedStdout is not DONT_CARE:
        expectedStdout = expectedStdout.replace("\r\n","\n")
        actualStdout = actualStdout.replace( "\r\n", "\n" )
        if expectedStdout != actualStdout:
            print("Stdout mismatch")
            printStatus(False)
            bad(groupNumber,testNumber)
            return False
            
         
    for fname,econtents in outputfiles:
        if not os.access(fname,os.F_OK):
            print("Expected to find output file",fname,"but did not")
            printStatus(False)
            bad(groupNumber,testNumber)
            return False
        with open(fname,errors="replace") as fp:
            acontents = fp.read()
        if econtents != acontents:
            print("Wrong contents in file",fname)
            printStatus(False)
            bad(groupNumber,testNumber)
            return False
 
        
    good(groupNumber,testNumber)
    return True


def main():
    global runOnly
    if len(sys.argv) > 1:
        gn = sys.argv[1]
        if gn.lower() == "none":
            gn = None
        else:
            gn = int(gn)
        ft = sys.argv[2]
        ft = int(ft)
        lt = sys.argv[3]
        if lt.lower() == "none":
            lt = None
        else:
            lt = int(lt)
    
        runOnly = ( gn, ft, lt )
        
    inputfile = os.path.join( os.path.dirname(__file__), "inputs.json" )
    with open(inputfile) as fp:
        indata = fp.read()
    indata = indata.replace("\r\n","\n")
    
    #replace '' ... '' as if it's a triple-quoted Python string.
    rex = re.compile(r"(?s)''(.*?)''")
    def repl(M):
        x = M.group(1)
        #literal backslash becomes double backslash
        x = x.replace("\\","\\\\")
        #newline becomes backslash followed by n
        x = x.replace("\n","\\n")
        #quotation mark becomes \"
        x = x.replace('"', '\\"' )
        #return valid JSON string
        return '"'+x+'"'
        
    indata = rex.sub( repl, indata )
    
    try:
        inputs = json.loads(indata)
    except json.JSONDecodeError:
        nl(indata)
        raise
    
    cases = []
    
    for testcase in inputs:
        #bonus 0 <-> group 0
        groupNumber = testcase.get("bonus",0)
        if groupNumber == False:
            groupNumber=0
        
        while groupNumber >= len(cases):
            cases.append( [] )
            numCases.append( 0 )
            numOK.append( 0 )
            numBad.append( 0 )
            groupsThatCannotContinue.append(False)

        cases[groupNumber].append(testcase)
        testcase["testNumber"] = len(cases[groupNumber])
        numCases[groupNumber] += 1

    allOK=True

    if runOnly[0] == None:
        groups = range(len(cases))
    else:
        groups = [runOnly[0]]
        
    groups = list(groups)
    print("Running groups:",groups)
    
    for groupNumber in groups:
        if groupsThatCannotContinue[groupNumber]:
            print("Abandoning the rest of group",groupNumber)
            continue
            
        ft,lt = runOnly[1:3]
        if lt == None:
            lt = len(cases[groupNumber])
        else:
            lt = lt+1
        tc = cases[groupNumber][ft:lt]
        
        print("Running",len(tc),"cases:",ft,"...",lt)
        
        for delta,testcase in enumerate(tc):
            ok = runOneTestcase(groupNumber,ft+delta,testcase)
            if not ok:
                allOK=False
    
    printSummary()
    quit()
    
main()
