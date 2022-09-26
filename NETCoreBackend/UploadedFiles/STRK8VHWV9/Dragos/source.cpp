#include <iostream>
#include <fstream>
using namespace std;

int main(int argc, char *argv[]) {	
	for(int i=0;i<argc;i++)
		cout << "argv[" << i << "] = " << argv[i] << endl;
	
	ifstream fin(argv[1]);
	ofstream fout(argv[2]);
	
	int a, b;
	fin >> a >> b;
	
	fout << a << endl;
	fout << b << endl;
	fout << a + b << endl;
	
	fin.close();
	fout.close();
	
    return 0;
}