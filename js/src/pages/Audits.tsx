import {FC, ReactElement, useContext, useEffect, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {AppContext} from "../App";
import {DataGrid} from "@material-ui/data-grid";

interface Audits {
    data: Audit[]
}

interface Audit {
    id: number,
    attributes: {
        subject: string,
        object: string,
        action: string,
        tenant: string
    }
}

const Audits: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const context = useContext(AppContext);

    const [audits, setAudits] = useState<Audits | null>(null);

    const columns = [
        { field: 'id', headerName: 'ID', width: 70 },
        { field: 'subject', headerName: 'Subject', width: 130 },
        { field: 'action', headerName: 'Action', width: 130 },
        { field: 'object', headerName: 'Object', width: 130 },
    ];

    useEffect(() => {
        fetch(context.settings.auditEndpoint, {
            headers: {
                'Accept': 'application/vnd.api+json'
            }
        })
            .then(response => response.json())
            .then(data => setAudits(data));
    }, [setAudits, context.settings.auditEndpoint]);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            {!audits && <div>Loading...</div>}
            {audits && <DataGrid
                rows={audits.data.map((a: Audit) => ({id: a.id, subject: a.attributes.subject, action: a.attributes.action, object: a.attributes.object}))}
                columns={columns}
                pageSize={5}
                rowsPerPageOptions={[5]}
                checkboxSelection
            />}

        </>
    );
}

export default Audits;