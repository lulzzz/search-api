psql -c 'CREATE DATABASE simsearch'
psql -c 'CREATE EXTENSION rdkit' simsearch
psql -c 'CREATE TABLE molecules_raw (id SERIAL, smiles text, idnumber text, name text, mw double precision, logp double precision, hba integer, hbd integer, rotb integer, tpsa double precision, fsp3 double precision, hac integer);' simsearch
tail -n +2 /src.txt | psql -c "copy molecules_raw (smiles, idnumber, name, mw, logp, hba, hbd, rotb, tpsa, fsp3, hac) from stdin with delimiter e'\t'" simsearch
psql -c 'select id, mol_from_smiles(smiles::cstring) mol, rdkit_fp(mol_from_smiles(smiles::cstring)) fp into mols from molecules_raw;' simsearch