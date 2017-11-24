package search.javaoplusd;

import chemaxon.formats.MolExporter;
import chemaxon.formats.MolFormatException;
import chemaxon.formats.MolImporter;
import chemaxon.struc.MolAtom;
import chemaxon.struc.MolBond;
import chemaxon.struc.Molecule;

public class Chem {
    public static String getOptimizedSmarts (String smarts) throws MolFormatException{
        
		Molecule mol = MolImporter.importMol(smarts);
		try {
            mol.aromatize();

            for (MolAtom atom : mol.getAtomArray()) {
                atom.clearQProps();
                if (atom.getAtno() == 1) {
                    for (MolBond bond : atom.getBondArray()) {
                        MolAtom atom1 = bond.getAtom1();
                        MolAtom atom2 = bond.getAtom2();
                        if (atom1.getAtno() == 1) {
                            String el = atom2.getExtraLabel();
                            if (el == null || el.isEmpty()) el = "0";
                            atom2.setExtraLabel(Integer.toString(Integer.parseInt(el)+ 1));
                        } else {
                            String el = atom1.getExtraLabel();
                            if (el == null || el.isEmpty()) el = "0";
                            atom1.setExtraLabel(Integer.toString(Integer.parseInt(el)+ 1));
                    }
                    }
                    mol.removeAtom(atom);
                }
            }
            
            for (MolAtom atom : mol.getAtomArray()) {
                String el = atom.getExtraLabel();
                if (el != null && !el.isEmpty()) {
                    atom.setQueryString("[H" + el + "]");
                }
            }

            return MolExporter.exportToFormat(mol, "smarts");
        } catch (Exception e) {
            return smarts;
        }
    }
}