psql -h rdkit-postgres -c 'DROP DATABASE IF EXISTS uorsy;'
psql -h rdkit-postgres -c 'CREATE DATABASE uorsy;'
psql -h rdkit-postgres uorsy << EndCreate
CREATE EXTENSION rdkit;
CREATE TABLE mr (id SERIAL primary key, ref text, smiles text);
EndCreate
cat ./src.txt | psql -h rdkit-postgres -d uorsy -c "COPY mr (ref, smiles) FROM STDIN WITH DELIMITER e'\t';"
psql -h rdkit-postgres uorsy << EOF
create index raw_ref_idx on mr using hash(ref);
select id, mol_from_smiles(smiles::cstring) mol, morganbv_fp(mol_from_smiles(smiles::cstring)) fp into ms from mr;
alter table ms add primary key (id);
alter table ms add foreign key (id) references mr;
create index ms_fp_idx on ms using gist(fp);
create index ms_mol_idx on ms using gist(mol);