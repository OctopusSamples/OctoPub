import {FC, ReactElement, useContext, useEffect, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {AppContext} from "../App";
import {DataGrid} from "@material-ui/data-grid";
import {getJsonApi} from "../utils/network";

interface AuditsCollection {
    data: Audit[]
}

interface Audit {
    id: number,
    attributes: {
        subject: string,
        object: string,
        action: string,
        dataPartition: string
    }
}

const Audits: FC<CommonProps> = (props: CommonProps): ReactElement => {
    props.setAllBookId(null);

    const context = useContext(AppContext);

    const [audits, setAudits] = useState<AuditsCollection | null>(null);
    const [error, setError] = useState<string | null>(null);

    const columns = [
        { field: 'id', headerName: 'Id', width: 70 },
        { field: 'subject', headerName: 'Subject', width: 130 },
        { field: 'action', headerName: 'Action', width: 600 },
        { field: 'object', headerName: 'Object', width: 130 },
    ];

    useEffect(() => {
        getJsonApi<AuditsCollection>(context.settings.auditEndpoint, props.partition)
            .then(data => setAudits(data))
            .catch(() => setError("Failed to retrieve audit resources."))
    }, [setAudits, context.settings.auditEndpoint, props.partition]);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            {!audits && !error && <div>Loading...</div>}
            {!audits && error && <div>{error}</div>}
            {audits && <DataGrid
                rows={audits.data.map((a: Audit) => ({id: a.id, subject: a.attributes.subject, action: a.attributes.action, object: a.attributes.object}))}
                columns={columns}
                pageSize={5}
                rowsPerPageOptions={[5]}
            />}

        </>
    );
}

export default Audits;