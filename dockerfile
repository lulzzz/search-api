FROM postgres:9.6

ENV http_proxy='http://10.3.80.80:3128'
ENV https_proxy='http://10.3.80.80:3128'

RUN apt-get update

RUN apt-get install -y \
    build-essential \
    cmake \
    sqlite3 \
    libsqlite3-dev \
    libboost-dev \
    libboost-system-dev \
    libboost-thread-dev \
    libboost-serialization-dev \
    libboost-regex-dev \
    postgresql-server-dev-9.6 \
    git

RUN git clone -b master --single-branch https://github.com/rdkit/rdkit.git

ENV RDBASE=/rdkit
ENV LD_LIBRARY_PATH=$RDBASE/lib:/usr/lib/x86_64-linux-gnu

RUN mkdir $RDBASE/build
WORKDIR $RDBASE/build

RUN cmake -DRDK_BUILD_PYTHON_WRAPPERS=OFF -DRDK_BUILD_PGSQL=ON -DPostgreSQL_TYPE_INCLUDE_DIR=/usr/include/postgresql/9.6/server/ -DPostgreSQL_ROOT=/usr ..
RUN make
RUN make install

RUN service postgresql stop
RUN sh Code/PgSQL/rdkit/pgsql_install.sh
RUN service postgresql start

WORKDIR $RDBASE
USER postgres

# RUN psql -c 'CREATE DATABASE simsearch'
# RUN psql -c 'CREATE EXTENSION rdkit' simsearch
# RUN psql -c 'CREATE TABLE molecules_raw (id SERIAL, smiles text, idnumber text, name text, mw double precision, logp double precision, hba integer, hbd integer, rotb integer, tpsa double precision, fsp3 double precision, hac integer);' simsearch
# RUN tail -n +2 /Intermed.txt | psql -c "copy molecules_raw (smiles, idnumber, name, mw, logp, hba, hbd, rotb, tpsa, fsp3, hac) from stdin with delimiter e'\t'" simsearch
# RUN psql -c 'select id, mol_from_smiles(smiles::cstring) mol, rdkit_fp(mol_from_smiles(smiles::cstring)) fp into mols from molecules_raw;' simsearch