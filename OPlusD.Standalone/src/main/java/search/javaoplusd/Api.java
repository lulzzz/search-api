package search.javaoplusd;

import java.util.List;

import javax.naming.OperationNotSupportedException;

import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.builder.SpringApplicationBuilder;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

@SpringBootApplication
@RestController
public class Api
{
	static Db _db;

	@RequestMapping(value = "/{type}/{target}", method = RequestMethod.GET, produces = "application/json")
	public List<Integer> sub(@PathVariable String type, @PathVariable String target, String smiles, int hitLimit) throws Exception {
		int typeInt;
		if(type.equals("sub")) {
			typeInt = Db.Types.SUB;
		}
		else if(type.equals("sup")) {
			typeInt = Db.Types.SUP;
		}
		else if(type.equals("sim")) {
			typeInt = Db.Types.SIM;
		}
		else {
			throw new OperationNotSupportedException(String.format("Search type %s is not supported", type));
		}

		int targetInt;
		if( target.equals("bb")) {
			targetInt = Db.Targets.BB;
		} else if( target.equals("sc")) {
			targetInt = Db.Targets.SC;
		} else {
			throw new OperationNotSupportedException(String.format("Search target %s is not supported", type));
		}

		String optimized = Chem.getOptimizedSmarts(smiles);		

		return _db.find(targetInt, typeInt, optimized, hitLimit);
	}

	public final static void main(String[] args)
	{
		String con = System.getenv("ora_connection");
		String user = System.getenv("ora_user"); // c$dcischem
		String pass = System.getenv("ora_pass"); // y3wxf1o(PLpt

		System.out.println(con);
		System.out.println(user);
		System.out.println(pass);

		if(con == null || con.length() == 0
		|| user == null || user.length() == 0
		|| pass == null || pass.length() == 0) {
			System.exit(-1);
		}

		_db = new Db(con, user, pass);

		new SpringApplicationBuilder(Api.class).run(args);
	}

}
