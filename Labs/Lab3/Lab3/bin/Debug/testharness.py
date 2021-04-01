#!/usr/bin/env python3

import json
import subprocess
import sys
import os
import os.path

#put the name of the executable here
#If you don't, the script will use the first exe it finds...
EXE=""



if not EXE:
    for dirpath,dirs,files in os.walk("."):
        for f in files:
            if f.endswith(".exe"):
                EXE=os.path.join(dirpath,f)
                print("Using",EXE)
                break
        else:
            continue
        break
                
    
def main():
    
    tpts=0
    ok = testWithFile("basictests.txt")
    if ok:
        print("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-Basic tests OK-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=")
    else:
        print("oops.")
        return
        
    tpts+=100
     
            
    print("Total pointage:",tpts)
            
        
    
def testWithFile(fname):
    with open(os.path.join(sys.path[0],fname)) as fp:
        data = fp.read()
    J = json.loads(data)
    for i in range(0,len(J),3):
        inp,expected,tree = J[i:i+3]
        print("Testing",inp,end=" ")
        sys.stdout.flush()
        cmd = []
        if "linux" in sys.platform:
            cmd.append("mono")
        cmd.append(EXE)
        P = subprocess.Popen(
                cmd,
                stdin=subprocess.PIPE,
                stdout=subprocess.PIPE,
            )
        o,_ = P.communicate( (inp+"\n").encode() )
            
        o = o.decode()
        try:
            actual = float(o)
        except ValueError as e:
            print("Not a float:",o)
            return False
        
        if not abs(expected-actual) < 0.00001:
            print("BAD!")
            print()
            print("Expected:",expected,"Actual:",actual)
            return False 
        else:
            print("     ", actual,"     OK!")
    return True

main()
