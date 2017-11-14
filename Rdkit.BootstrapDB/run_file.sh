psql -h rdkit-postgres -c 'DROP DATABASE IF EXISTS simsearch;'
psql -h rdkit-postgres -c 'CREATE DATABASE simsearch;'
psql -h rdkit-postgres simsearch << EndCreate
CREATE EXTENSION rdkit;
CREATE TABLE molecules_raw (id SERIAL primary key, smiles text, idnumber text, name text, mw double precision, logp double precision, hba integer, hbd integer, rotb integer, tpsa double precision, fsp3 double precision, hac integer);
EndCreate
tail -n +2 ./src.txt | psql -h rdkit-postgres -d simsearch -c "COPY molecules_raw (smiles, idnumber, name, mw, logp, hba, hbd, rotb, tpsa, fsp3, hac) FROM STDIN WITH DELIMITER e'\t';"
psql -h rdkit-postgres simsearch << EOF
create index raw_sml_idx on molecules_raw using hash(smiles);
create index raw_name_idx on molecules_raw using hash(name);
create index raw_idnum_idx on molecules_raw using hash(idnumber);
select id, mol_from_smiles(smiles::cstring) mol, morganbv_fp(mol_from_smiles(smiles::cstring)) fp into mols from molecules_raw;
alter table mols add primary key (id);
alter table mols add foreign key(id) references molecules_raw;
create index mols_fp_idx on mols using gist(fp);
create index mols_mol_idx on mols using gist(mol);